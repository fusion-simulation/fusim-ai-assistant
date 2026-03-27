using System.Text;
using System.Text.Json;
using FusimAiAssiant.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FusimAiAssiant.Services;

public sealed class SubmitParameterChatAgentService : ISubmitParameterChatAgentService
{
    private const int MaxConversationHistoryMessages = 12;

    private readonly IServiceProvider _serviceProvider;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly VmomInputDraftService _inputDraftService;
    private readonly ILogger<SubmitParameterChatAgentService> _logger;

    public SubmitParameterChatAgentService(
        IServiceProvider serviceProvider,
        IChatCompletionService chatCompletionService,
        VmomInputDraftService inputDraftService,
        ILogger<SubmitParameterChatAgentService> logger)
    {
        _serviceProvider = serviceProvider;
        _chatCompletionService = chatCompletionService;
        _inputDraftService = inputDraftService;
        _logger = logger;
    }

    public async Task<SubmitAgentChatResponse> ChatAsync(
        SubmitAgentChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return new SubmitAgentChatResponse("请输入问题内容。", [], null, null);
        }

        var draft = CreateDraft(request);
        var kernel = new Kernel(_serviceProvider);
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            你是 VMOM 提交前参数调优助手。你只能基于用户当前输入的参数回答，不可编造运行结果。
            你只能建议修改当前已知字段，不允许创造新参数名。
            回答使用中文，先说明原因，再给出建议。
            你的输出必须符合结构化响应格式。
            """);
        chatHistory.AddUserMessage(BuildDraftContextPrompt(request.Mode, request.Title, draft.Fields, draft.InputContent));

        foreach (var message in BuildConversationHistory(request.History, request.Message))
        {
            if (string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
            {
                chatHistory.AddAssistantMessage(message.Content);
                continue;
            }

            chatHistory.AddUserMessage(message.Content);
        }

#pragma warning disable SKEXP0010
        var settings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = typeof(SubmitAgentStructuredReply)
        };
#pragma warning restore SKEXP0010

        try
        {
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                settings,
                kernel,
                cancellationToken);

            var parsed = ParseStructuredReply(response.Content);
            return new SubmitAgentChatResponse(
                parsed.Answer,
                FilterChanges(parsed.ProposedChanges, draft.Fields),
                draft.InputContent,
                draft.Fields);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Submit parameter chat completion failed.");
            return new SubmitAgentChatResponse(
                "调参服务暂时不可用，请稍后重试。",
                [],
                draft.InputContent,
                draft.Fields);
        }
    }

    private VmomInputDraft CreateDraft(SubmitAgentChatRequest request)
    {
        if (string.Equals(request.Mode, "ByInputFile", StringComparison.OrdinalIgnoreCase))
        {
            return _inputDraftService.ParseEqinpt(request.InputContent ?? string.Empty);
        }

        return _inputDraftService.CreateDraftFromFields(request.Fields ?? new Dictionary<string, string>());
    }

    private static IReadOnlyList<CaseAgentChatMessage> BuildConversationHistory(
        IReadOnlyList<CaseAgentChatMessage>? history,
        string currentMessage)
    {
        var normalizedHistory = (history ?? Array.Empty<CaseAgentChatMessage>())
            .Where(message => !string.IsNullOrWhiteSpace(message.Content))
            .Select(message => new CaseAgentChatMessage(
                string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user",
                message.Content.Trim(),
                message.ImageUrl))
            .TakeLast(MaxConversationHistoryMessages)
            .ToList();

        if (!string.IsNullOrWhiteSpace(currentMessage))
        {
            normalizedHistory.Add(new CaseAgentChatMessage("user", currentMessage.Trim(), null));
        }

        return normalizedHistory;
    }

    private static string BuildDraftContextPrompt(
        string mode,
        string title,
        IReadOnlyDictionary<string, string> fields,
        string inputContent)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Mode={mode}");
        sb.AppendLine($"Title={title}");
        sb.AppendLine("Fields:");

        foreach (var field in fields.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            sb.AppendLine($"{field.Key} = {field.Value}");
        }

        sb.AppendLine("NormalizedInput:");
        sb.AppendLine(inputContent);
        return sb.ToString();
    }

    private static SubmitAgentStructuredReply ParseStructuredReply(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new SubmitAgentStructuredReply
            {
                Answer = "我没有从模型获得有效回复，请重试。"
            };
        }

        try
        {
            return JsonSerializer.Deserialize<SubmitAgentStructuredReply>(
                       content,
                       new JsonSerializerOptions(JsonSerializerDefaults.Web))
                   ?? new SubmitAgentStructuredReply
                   {
                       Answer = content.Trim()
                   };
        }
        catch (JsonException)
        {
            return new SubmitAgentStructuredReply
            {
                Answer = content.Trim()
            };
        }
    }

    private static IReadOnlyList<SubmitAgentProposedChange> FilterChanges(
        IReadOnlyList<SubmitAgentStructuredChange>? changes,
        IReadOnlyDictionary<string, string> fields)
    {
        var filtered = new List<SubmitAgentProposedChange>();

        foreach (var change in changes ?? [])
        {
            if (string.IsNullOrWhiteSpace(change.FieldKey))
            {
                continue;
            }

            var key = change.FieldKey.Trim();
            if (!fields.TryGetValue(key, out var currentValue))
            {
                continue;
            }

            filtered.Add(new SubmitAgentProposedChange(
                key,
                currentValue,
                change.SuggestedValue?.Trim() ?? string.Empty,
                change.Reason?.Trim() ?? string.Empty));
        }

        return filtered;
    }

    private sealed class SubmitAgentStructuredReply
    {
        public string Answer { get; set; } = string.Empty;

        public List<SubmitAgentStructuredChange> ProposedChanges { get; set; } = [];
    }

    private sealed class SubmitAgentStructuredChange
    {
        public string FieldKey { get; set; } = string.Empty;

        public string SuggestedValue { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}

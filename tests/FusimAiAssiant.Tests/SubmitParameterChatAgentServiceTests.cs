using System.Reflection;
using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class SubmitParameterChatAgentServiceTests
{
    [Fact]
    public void BuildConversationHistory_KeepsRecentTurns_AndCurrentPrompt()
    {
        var method = typeof(SubmitParameterChatAgentService).GetMethod(
            "BuildConversationHistory",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var history = Enumerable.Range(1, 15)
            .Select(index => new CaseAgentChatMessage("user", $"消息{index}", null))
            .ToArray();

        var result = method!.Invoke(null, new object?[] { history, "帮我调稳一点" });
        var messages = Assert.IsAssignableFrom<IReadOnlyList<CaseAgentChatMessage>>(result);

        Assert.Equal(13, messages.Count);
        Assert.Equal("消息4", messages[0].Content);
        Assert.Equal("消息15", messages[^2].Content);
        Assert.Equal("帮我调稳一点", messages[^1].Content);
    }

    [Fact]
    public void BuildDraftContextPrompt_ContainsMode_Title_AndKnownFields()
    {
        var method = typeof(SubmitParameterChatAgentService).GetMethod(
            "BuildDraftContextPrompt",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var prompt = Assert.IsType<string>(method!.Invoke(null, new object?[]
        {
            "ByForm",
            "case-a",
            new Dictionary<string, string>
            {
                ["rmajor"] = "7.9",
                ["elong"] = "1.5"
            },
            "&eqinpt\nrmajor = 7.9,\nelong = 1.5,\n/"
        }));

        Assert.Contains("Mode=ByForm", prompt, StringComparison.Ordinal);
        Assert.Contains("Title=case-a", prompt, StringComparison.Ordinal);
        Assert.Contains("rmajor = 7.9", prompt, StringComparison.Ordinal);
        Assert.Contains("&eqinpt", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSystemPrompt_ExplicitlyRequiresJsonOutput()
    {
        var method = typeof(SubmitParameterChatAgentService).GetMethod(
            "BuildSystemPrompt",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var prompt = Assert.IsType<string>(method!.Invoke(null, null));

        Assert.Contains("JSON", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("answer", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("proposedChanges", prompt, StringComparison.OrdinalIgnoreCase);
    }
}

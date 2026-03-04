using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using FusimAiAssiant.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FusimAiAssiant.Services;

public sealed class CaseDetailChatAgentService : ICaseDetailChatAgentService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly IVmomCaseService _caseService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ILogger<CaseDetailChatAgentService> _logger;

    public CaseDetailChatAgentService(
        IVmomCaseService caseService,
        IServiceProvider serviceProvider,
        IChatCompletionService chatCompletionService,
        ILogger<CaseDetailChatAgentService> logger)
    {
        _caseService = caseService;
        _serviceProvider = serviceProvider;
        _chatCompletionService = chatCompletionService;
        _logger = logger;
    }

    public async Task<CaseAgentChatResponse> ChatAsync(int caseId, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return new CaseAgentChatResponse("请输入问题内容。", null, null, null, null);
        }

        var workspace = await _caseService.GetCaseWorkspaceAsync(caseId, cancellationToken);
        if (workspace is null)
        {
            return new CaseAgentChatResponse("未找到算例工作目录，无法进行问答。", null, null, null, null);
        }

        var detail = await _caseService.GetCaseDetailAsync(caseId, cancellationToken);
        if (detail is null)
        {
            return new CaseAgentChatResponse("未找到该算例详情。", null, null, null, workspace.Files);
        }

        var kernel = new Kernel(_serviceProvider);
        var plotTools = new CasePlotTools(caseId, workspace, _logger);
        kernel.ImportPluginFromObject(plotTools, "vmom_plot");

        var history = new ChatHistory();
        history.AddSystemMessage(
            """
            你是 VMOM 算例分析助手。你必须基于算例上下文回答，不可编造。
            当用户要求绘图、画曲线、plot某个文件变量时，调用 vmom_plot 工具完成。
            如用户未提供文件名或变量，先调用工具查询可用文件或变量，再给出明确建议。
            回答使用中文，简洁明确。
            """);
        history.AddUserMessage(BuildPrompt(detail, workspace.Files, message));

#pragma warning disable SKEXP0010
        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
#pragma warning restore SKEXP0010

        try
        {
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                history,
                settings,
                kernel,
                cancellationToken);

            var answer = string.IsNullOrWhiteSpace(response.Content)
                ? "我没有从模型获得有效回复，请重试。"
                : response.Content.Trim();

            if (plotTools.PlotAttempted && !string.IsNullOrWhiteSpace(plotTools.LastPlotError))
            {
                var errorDetail = plotTools.LastPlotError!;
                return new CaseAgentChatResponse(
                    $"绘图失败，gnuplot 错误如下：\n{errorDetail}",
                    null,
                    null,
                    plotTools.LastColumns,
                    workspace.Files);
            }

            return new CaseAgentChatResponse(
                answer,
                plotTools.LastImageUrl,
                plotTools.LastImageFileName,
                plotTools.LastColumns,
                workspace.Files);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Case chat completion failed.");
            return new CaseAgentChatResponse("问答服务暂时不可用，请稍后重试。", null, null, null, workspace.Files);
        }
    }

    private static string BuildPrompt(VmomCaseDetail detail, IReadOnlyList<string> files, string userMessage)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CaseId={detail.Id}, Status={detail.Status}, Title={detail.Title}");
        sb.AppendLine($"AvailableFiles: {string.Join(", ", files)}");
        sb.AppendLine("RZ.txt:");
        sb.AppendLine(TrimForPrompt(detail.RzText, 3000));
        sb.AppendLine("eqpr_iota.txt:");
        sb.AppendLine(TrimForPrompt(detail.EqprIotaText, 3000));
        sb.AppendLine("vmom.out:");
        sb.AppendLine(TrimForPrompt(detail.VmomOutText, 3000));
        sb.AppendLine();
        sb.AppendLine($"UserMessage: {userMessage}");
        return sb.ToString();
    }

    private static string TrimForPrompt(string text, int maxChars)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text.Length <= maxChars ? text : text[..maxChars];
    }

    private sealed class CasePlotTools
    {
        private readonly int _caseId;
        private readonly VmomCaseWorkspace _workspace;
        private readonly ILogger _logger;

        public CasePlotTools(int caseId, VmomCaseWorkspace workspace, ILogger logger)
        {
            _caseId = caseId;
            _workspace = workspace;
            _logger = logger;
        }

        public string? LastImageUrl { get; private set; }
        public string? LastImageFileName { get; private set; }
        public IReadOnlyList<string>? LastColumns { get; private set; }
        public bool PlotAttempted { get; private set; }
        public string? LastPlotError { get; private set; }

        [KernelFunction, Description("列出当前算例工作目录中的数据文件")]
        public string ListDataFiles()
        {
            return _workspace.Files.Count == 0 ? "No files found." : string.Join(", ", _workspace.Files);
        }

        [KernelFunction, Description("读取某个数据文件的可用变量列名")]
        public async Task<string> ListVariablesAsync(
            [Description("数据文件名，例如 eqpr_iota.txt")] string fileName,
            CancellationToken cancellationToken = default)
        {
            var filePath = GetSafeFilePath(_workspace.WorkDirectory, fileName);
            if (filePath is null || !File.Exists(filePath))
            {
                return $"File not found: {fileName}";
            }

            var analysis = await AnalyzeTableAsync(filePath, cancellationToken);
            LastColumns = analysis.Columns;
            return analysis.Columns.Count == 0
                ? $"No variable columns in {fileName}."
                : string.Join(", ", analysis.Columns);
        }

        [KernelFunction, Description("对指定文件的两个变量绘制曲线图，生成 png 图片并返回访问地址")]
        public async Task<string> PlotVariablesAsync(
            [Description("数据文件名，例如 eqpr_iota.txt")] string fileName,
            [Description("x轴变量名或列序号，例 r 或 col1 或 1")] string xVariable,
            [Description("y轴变量名或列序号，例 q 或 col2 或 2")] string yVariable,
            CancellationToken cancellationToken = default)
        {
            PlotAttempted = true;
            LastPlotError = null;
            LastImageUrl = null;
            LastImageFileName = null;

            var filePath = GetSafeFilePath(_workspace.WorkDirectory, fileName);
            if (filePath is null || !File.Exists(filePath))
            {
                LastPlotError = $"file not found: {fileName}";
                return $"Plot failed: file not found: {fileName}";
            }

            var analysis = await AnalyzeTableAsync(filePath, cancellationToken);
            LastColumns = analysis.Columns;

            if (!analysis.HasNumericData || analysis.Columns.Count < 2)
            {
                LastPlotError = $"{fileName} has no plottable numeric table.";
                return $"Plot failed: {fileName} has no plottable numeric table.";
            }

            var xIndex = ResolveColumnIndex(xVariable, analysis.Columns);
            var yIndex = ResolveColumnIndex(yVariable, analysis.Columns);
            if (xIndex is null || yIndex is null || xIndex == yIndex)
            {
                LastPlotError = $"invalid variables. Available columns: {string.Join(", ", analysis.Columns)}";
                return $"Plot failed: invalid variables. Available columns: {string.Join(", ", analysis.Columns)}";
            }

            var xName = analysis.Columns[xIndex.Value];
            var yName = analysis.Columns[yIndex.Value];

            var plotDirectory = Path.Combine(_workspace.WorkDirectory, "plots");
            Directory.CreateDirectory(plotDirectory);

            var imageFileName =
                $"{Path.GetFileNameWithoutExtension(fileName)}-{Sanitize(yName)}-vs-{Sanitize(xName)}-{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var imagePath = Path.Combine(plotDirectory, imageFileName);

            var result = await RunGnuplotAsync(filePath, imagePath, xIndex.Value + 1, yIndex.Value + 1, xName, yName, cancellationToken);
            if (!result.Success)
            {
                _logger.LogWarning("Gnuplot failed for case {CaseId}: {Error}", _caseId, result.ErrorMessage);
                LastPlotError = result.ErrorMessage;
                return $"Plot failed: {result.ErrorMessage}";
            }

            LastImageFileName = imageFileName;
            LastImageUrl = $"/api/vmom/cases/{_caseId}/plots/{Uri.EscapeDataString(imageFileName)}?ts={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            return $"Plot created. imageUrl={LastImageUrl}; x={xName}; y={yName}; file={fileName}";
        }

        private static string? GetSafeFilePath(string workDirectory, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var exact = Directory.GetFiles(workDirectory, "*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path => string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase));

            if (exact is null)
            {
                return null;
            }

            var root = Path.GetFullPath(workDirectory);
            var fullPath = Path.GetFullPath(exact);
            return fullPath.StartsWith(root, StringComparison.Ordinal) ? fullPath : null;
        }

        private static int? ResolveColumnIndex(string variable, IReadOnlyList<string> columns)
        {
            if (string.IsNullOrWhiteSpace(variable))
            {
                return null;
            }

            var text = variable.Trim();
            var exact = columns
                .Select((name, index) => new { name, index })
                .FirstOrDefault(item => item.name.Equals(text, StringComparison.OrdinalIgnoreCase));
            if (exact is not null)
            {
                return exact.index;
            }

            if (int.TryParse(text, out var number) && number >= 1 && number <= columns.Count)
            {
                return number - 1;
            }

            if (text.StartsWith("col", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(text[3..], out var colNumber)
                && colNumber >= 1
                && colNumber <= columns.Count)
            {
                return colNumber - 1;
            }

            return null;
        }

        private static async Task<TableAnalysisResult> AnalyzeTableAsync(string filePath, CancellationToken cancellationToken)
        {
            var columns = new List<string>();
            var hasHeader = false;
            var hasNumericData = false;

            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cancellationToken) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var trimmed = line.Trim();
                if (trimmed.StartsWith("#", StringComparison.Ordinal)
                    || trimmed.StartsWith("!", StringComparison.Ordinal)
                    || trimmed.StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                var tokens = trimmed.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Length < 2)
                {
                    continue;
                }

                if (!hasHeader)
                {
                    var numericMask = tokens.Select(IsNumericToken).ToArray();
                    var allNumeric = numericMask.All(static v => v);
                    if (!allNumeric)
                    {
                        hasHeader = true;
                        columns.AddRange(tokens.Select(SanitizeColumnName));
                        continue;
                    }

                    hasHeader = true;
                    columns.AddRange(Enumerable.Range(1, tokens.Length).Select(i => $"col{i}"));
                    hasNumericData = true;
                    continue;
                }

                if (tokens.Any(IsNumericToken))
                {
                    hasNumericData = true;
                    if (tokens.Length > columns.Count)
                    {
                        columns.AddRange(Enumerable.Range(columns.Count + 1, tokens.Length - columns.Count).Select(i => $"col{i}"));
                    }
                }
            }

            return new TableAnalysisResult(columns, hasNumericData);
        }

        private static bool IsNumericToken(string token)
        {
            var normalized = token.Replace('D', 'E').Replace('d', 'e');
            return double.TryParse(normalized, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _);
        }

        private static string SanitizeColumnName(string token)
        {
            var cleaned = token.Trim();
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                return "col";
            }

            var chars = cleaned.Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-').ToArray();
            return chars.Length == 0 ? "col" : new string(chars);
        }

        private static async Task<GnuplotResult> RunGnuplotAsync(
            string dataFilePath,
            string outputImagePath,
            int xColumn,
            int yColumn,
            string xLabel,
            string yLabel,
            CancellationToken cancellationToken)
        {
            var plotScriptPath = Path.Combine(
                Path.GetDirectoryName(outputImagePath)!,
                $"{Path.GetFileNameWithoutExtension(outputImagePath)}.plt");

            var script =
                $"""
                 set terminal pngcairo size 1200,700
                 set output '{EscapeForGnuplot(outputImagePath)}'
                 set datafile commentschars '#'
                 set grid
                 set xlabel '{EscapeForGnuplotText(xLabel)}'
                 set ylabel '{EscapeForGnuplotText(yLabel)}'
                 set title '{EscapeForGnuplotText(yLabel)} vs {EscapeForGnuplotText(xLabel)}'
                 plot '{EscapeForGnuplot(dataFilePath)}' using {xColumn}:{yColumn} with lines lw 2 lc rgb '#f5a623' title '{EscapeForGnuplotText(yLabel)}'
                 """;

            await File.WriteAllTextAsync(plotScriptPath, script, Utf8WithoutBom, cancellationToken);

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "gnuplot",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                startInfo.ArgumentList.Add(plotScriptPath);

                using var process = new Process { StartInfo = startInfo };
                if (!process.Start())
                {
                    return new GnuplotResult(false, "gnuplot failed to start.");
                }

                var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);
                var stdout = await stdoutTask;
                var stderr = await stderrTask;

                if (process.ExitCode != 0 || !File.Exists(outputImagePath))
                {
                    var message = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                    var normalized = string.IsNullOrWhiteSpace(message) ? "gnuplot did not produce image." : message.Trim();
                    var compact = normalized.Length <= 3000 ? normalized : normalized[..3000];
                    return new GnuplotResult(false, compact);
                }

                return new GnuplotResult(true, null);
            }
            catch (Exception ex)
            {
                return new GnuplotResult(false, ex.Message);
            }
            finally
            {
                try
                {
                    File.Delete(plotScriptPath);
                }
                catch
                {
                    // Ignore temp file cleanup failures.
                }
            }
        }

        private static string EscapeForGnuplot(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'");
        }

        private static string EscapeForGnuplotText(string value)
        {
            return value.Replace("'", "\\'");
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "var";
            }

            var chars = value.Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-').ToArray();
            return chars.Length == 0 ? "var" : new string(chars);
        }

        private sealed record TableAnalysisResult(IReadOnlyList<string> Columns, bool HasNumericData);
        private sealed record GnuplotResult(bool Success, string? ErrorMessage);
    }
}

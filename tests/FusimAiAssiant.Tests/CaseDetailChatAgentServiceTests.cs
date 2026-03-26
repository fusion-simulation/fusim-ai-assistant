using System.Reflection;
using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class CaseDetailChatAgentServiceTests
{
    [Fact]
    public void BuildConversationHistory_PreservesTurnsAndIgnoresBlankItems()
    {
        var buildHistoryMethod = typeof(CaseDetailChatAgentService).GetMethod(
            "BuildConversationHistory",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(buildHistoryMethod);

        var history = new[]
        {
            new CaseAgentChatMessage("user", "先看一下 RZ.txt", null),
            new CaseAgentChatMessage("assistant", "这是第一轮解释。", null),
            new CaseAgentChatMessage("assistant", "   ", null),
            new CaseAgentChatMessage("user", "", null)
        };

        var result = buildHistoryMethod!.Invoke(null, new object?[] { history, "继续分析 vmom.out" });

        var messages = Assert.IsAssignableFrom<IReadOnlyList<CaseAgentChatMessage>>(result);
        Assert.Equal(3, messages.Count);
        Assert.Equal(("user", "先看一下 RZ.txt"), (messages[0].Role, messages[0].Content));
        Assert.Equal(("assistant", "这是第一轮解释。"), (messages[1].Role, messages[1].Content));
        Assert.Equal(("user", "继续分析 vmom.out"), (messages[2].Role, messages[2].Content));
    }

    [Fact]
    public void BuildConversationHistory_KeepsOnlyRecentTurnsBeforeCurrentMessage()
    {
        var buildHistoryMethod = typeof(CaseDetailChatAgentService).GetMethod(
            "BuildConversationHistory",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(buildHistoryMethod);

        var history = Enumerable.Range(1, 15)
            .Select(index => new CaseAgentChatMessage("user", $"消息{index}", null))
            .ToArray();

        var result = buildHistoryMethod!.Invoke(null, new object?[] { history, "最新问题" });

        var messages = Assert.IsAssignableFrom<IReadOnlyList<CaseAgentChatMessage>>(result);
        Assert.Equal(13, messages.Count);
        Assert.Equal("消息4", messages[0].Content);
        Assert.Equal("消息15", messages[^2].Content);
        Assert.Equal("最新问题", messages[^1].Content);
    }

    [Fact]
    public async Task AnalyzeTableAsync_UsesStableGeneratedColumns_ForHeaderlessNumericTable()
    {
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(
            tempFile,
            """
            1 2 3
            4 5 6
            7 8 9
            """);

        try
        {
            var casePlotToolsType = typeof(CaseDetailChatAgentService)
                .GetNestedType("CasePlotTools", BindingFlags.NonPublic);
            Assert.NotNull(casePlotToolsType);

            var analyzeMethod = casePlotToolsType!.GetMethod(
                "AnalyzeTableAsync",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(analyzeMethod);

            var taskObj = analyzeMethod!.Invoke(null, new object[] { tempFile, CancellationToken.None });
            Assert.NotNull(taskObj);

            var task = Assert.IsAssignableFrom<Task>(taskObj);
            await task;

            var result = taskObj!.GetType().GetProperty("Result")!.GetValue(taskObj);
            Assert.NotNull(result);

            var columns = Assert.IsAssignableFrom<IReadOnlyList<string>>(
                result!.GetType().GetProperty("Columns")!.GetValue(result));
            var hasNumericData = Assert.IsType<bool>(
                result.GetType().GetProperty("HasNumericData")!.GetValue(result));

            Assert.Equal(new[] { "col1", "col2", "col3" }, columns);
            Assert.True(hasNumericData);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

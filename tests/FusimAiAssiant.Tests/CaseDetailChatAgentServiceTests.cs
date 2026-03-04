using System.Reflection;
using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class CaseDetailChatAgentServiceTests
{
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

using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class ChatMessageFormatterTests
{
    [Fact]
    public void NormalizeAssistantContent_RemovesMarkdownImage_WhenImageUrlExists()
    {
        const string content = """
                               这是图像结果：
                               ![plot](/api/vmom/cases/1/plots/a.png)

                               趋势是下降。
                               """;

        var normalized = ChatMessageFormatter.NormalizeAssistantContent(content, "/api/vmom/cases/1/plots/a.png");

        Assert.DoesNotContain("![plot]", normalized, StringComparison.Ordinal);
        Assert.DoesNotContain("<img", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("趋势是下降。", normalized, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizeAssistantContent_KeepsMarkdownImage_WhenImageUrlMissing()
    {
        const string content = "![plot](/api/vmom/cases/1/plots/a.png)";

        var normalized = ChatMessageFormatter.NormalizeAssistantContent(content, null);

        Assert.Equal(content, normalized);
    }
}

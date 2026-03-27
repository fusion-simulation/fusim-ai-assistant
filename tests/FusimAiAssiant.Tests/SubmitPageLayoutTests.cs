using System.Text.RegularExpressions;
using Xunit;

namespace FusimAiAssiant.Tests;

public class SubmitPageLayoutTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../"));

    [Fact]
    public void SubmitPage_UsesDedicatedScrollAndStickyLayoutContainers()
    {
        var markup = File.ReadAllText(Path.Combine(RepoRoot, "Pages", "Submit.razor"));

        Assert.Contains("submit-form-column", markup);
        Assert.Contains("submit-agent-shell", markup);
    }

    [Fact]
    public void SubmitPageCss_DefinesViewportPinnedAssistantAndScrollableFormColumn()
    {
        var css = File.ReadAllText(Path.Combine(RepoRoot, "Pages", "Submit.razor.css"));

        Assert.Matches(new Regex(@"\.submit-page-viewport\s*\{[\s\S]*box-sizing:\s*border-box;", RegexOptions.Multiline), css);
        Assert.Matches(new Regex(@"\.submit-page\s*\{[\s\S]*min-height:\s*100%;", RegexOptions.Multiline), css);
        Assert.Matches(new Regex(@"\.submit-form-column\s*\{[\s\S]*min-height:\s*100%;", RegexOptions.Multiline), css);
        Assert.Matches(new Regex(@"\.submit-form-column\s*\{[\s\S]*overflow-y:\s*auto;", RegexOptions.Multiline), css);
        Assert.Matches(new Regex(@"\.submit-agent-shell\s*\{[\s\S]*height:\s*100%;", RegexOptions.Multiline), css);
        Assert.Matches(new Regex(@"@media\s*\(max-width:\s*1024px\)[\s\S]*\.submit-agent-shell\s*\{[\s\S]*height:\s*auto;", RegexOptions.Multiline), css);
    }
}

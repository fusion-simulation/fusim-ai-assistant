using System.Text.RegularExpressions;

namespace FusimAiAssiant.Services;

public static class ChatMessageFormatter
{
    private static readonly Regex MarkdownImageRegex = new(
        @"!\[[^\]]*\]\([^\)\r\n]*\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex HtmlImageRegex = new(
        @"<img\b[^>]*>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex ExcessiveNewlinesRegex = new(
        @"(\r?\n){3,}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string NormalizeAssistantContent(string? content, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return content;
        }

        var cleaned = MarkdownImageRegex.Replace(content, string.Empty);
        cleaned = HtmlImageRegex.Replace(cleaned, string.Empty);
        cleaned = ExcessiveNewlinesRegex.Replace(cleaned, Environment.NewLine + Environment.NewLine);
        return cleaned.Trim();
    }
}

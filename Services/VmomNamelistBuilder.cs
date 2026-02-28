using System.Text;

namespace FusimAiAssiant.Services;

public class VmomNamelistBuilder
{
    private const int MaxLineLength = 72;

    public string BuildEqinptNamelist(IReadOnlyDictionary<string, string> fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("&eqinpt");

        foreach (var (key, rawValue) in fields)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(rawValue))
            {
                continue;
            }

            var value = NormalizeValue(rawValue);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            AppendAssignment(sb, key.Trim(), value);
        }

        sb.AppendLine("/");
        return sb.ToString();
    }

    private static string NormalizeValue(string raw)
    {
        return raw
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
    }

    private static bool IsArrayValue(string value)
    {
        return value.Contains(',', StringComparison.Ordinal);
    }

    private void AppendAssignment(StringBuilder sb, string key, string value)
    {
        if (!IsArrayValue(value))
        {
            var scalarLine = $"{key} = {value},";
            if (scalarLine.Length <= MaxLineLength)
            {
                sb.AppendLine(scalarLine);
            }
            else
            {
                AppendWrapped(sb, key, [value]);
            }
            return;
        }

        var tokens = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();

        AppendWrapped(sb, key, tokens);
    }

    private void AppendWrapped(StringBuilder sb, string key, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            return;
        }

        var prefix = $"{key} = ";
        var indent = new string(' ', prefix.Length);
        var current = prefix;

        for (var index = 0; index < tokens.Count; index++)
        {
            var segment = $"{tokens[index]},";
            var candidate = current.Length == prefix.Length
                ? current + segment
                : current + " " + segment;

            if (candidate.Length <= MaxLineLength)
            {
                current = candidate;
                continue;
            }

            if (current.Length > prefix.Length)
            {
                sb.AppendLine(current);
                current = indent + segment;
                continue;
            }

            // Fallback for a single token exceeding the line limit.
            sb.AppendLine(candidate);
            current = indent;
        }

        if (current.Trim().Length > 0)
        {
            sb.AppendLine(current);
        }
    }
}

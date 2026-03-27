using FusimAiAssiant.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace FusimAiAssiant.Services;

public sealed record VmomInputDraft(
    IReadOnlyDictionary<string, string> Fields,
    string InputContent,
    IReadOnlyList<string> RejectedKeys);

public sealed class VmomInputDraftService
{
    private static readonly Regex AssignmentRegex = new(
        @"(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*=",
        RegexOptions.Compiled);

    private readonly VmomNamelistBuilder _namelistBuilder;

    public VmomInputDraftService(VmomNamelistBuilder namelistBuilder)
    {
        _namelistBuilder = namelistBuilder;
    }

    public VmomInputDraft ParseEqinpt(string inputContent)
    {
        var originalInput = inputContent ?? string.Empty;
        var fields = ParseEqinptFields(originalInput);
        return new VmomInputDraft(
            fields,
            originalInput,
            []);
    }

    public VmomInputDraft CreateDraftFromFields(IReadOnlyDictionary<string, string> fields)
    {
        var normalizedFields = NormalizeFields(fields);
        return new VmomInputDraft(
            normalizedFields,
            _namelistBuilder.BuildEqinptNamelist(normalizedFields),
            []);
    }

    public VmomInputDraft ApplyChanges(
        VmomInputDraft draft,
        IReadOnlyList<SubmitAgentProposedChange> changes)
    {
        var fields = NormalizeFields(draft.Fields);
        var rejectedKeys = new List<string>();

        foreach (var change in changes ?? Array.Empty<SubmitAgentProposedChange>())
        {
            if (string.IsNullOrWhiteSpace(change.FieldKey))
            {
                continue;
            }

            var key = NormalizeKey(change.FieldKey);
            if (!fields.ContainsKey(key))
            {
                rejectedKeys.Add(key);
                continue;
            }

            if (string.IsNullOrWhiteSpace(change.SuggestedValue))
            {
                continue;
            }

            fields[key] = change.SuggestedValue.Trim();
        }

        return new VmomInputDraft(
            fields,
            _namelistBuilder.BuildEqinptNamelist(fields),
            rejectedKeys);
    }

    private static Dictionary<string, string> NormalizeFields(IReadOnlyDictionary<string, string> fields)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in fields)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[NormalizeKey(key)] = value?.Trim() ?? string.Empty;
        }

        return result;
    }

    private static Dictionary<string, string> ParseEqinptFields(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var eqinptBody = ExtractEqinptBody(content ?? string.Empty);
        if (string.IsNullOrWhiteSpace(eqinptBody))
        {
            return result;
        }

        var matches = AssignmentRegex.Matches(eqinptBody);
        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var nextIndex = i + 1 < matches.Count ? matches[i + 1].Index : eqinptBody.Length;
            var valueSegment = eqinptBody[(match.Index + match.Length)..nextIndex];
            var value = valueSegment.Trim().TrimEnd(',').Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            result[NormalizeKey(match.Groups["key"].Value)] = value;
        }

        return result;
    }

    private static string ExtractEqinptBody(string content)
    {
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var sb = new StringBuilder();
        var inEqinpt = false;

        foreach (var rawLine in lines)
        {
            var line = StripInlineComment(rawLine);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!inEqinpt)
            {
                var marker = line.IndexOf("&eqinpt", StringComparison.OrdinalIgnoreCase);
                if (marker < 0)
                {
                    continue;
                }

                inEqinpt = true;
                line = line[(marker + "&eqinpt".Length)..].Trim();
                if (line.Length == 0)
                {
                    continue;
                }
            }

            var slashIndex = line.IndexOf('/');
            if (slashIndex >= 0)
            {
                var beforeSlash = line[..slashIndex].Trim();
                if (beforeSlash.Length > 0)
                {
                    AppendSegment(sb, beforeSlash);
                }

                break;
            }

            AppendSegment(sb, line);
        }

        return sb.ToString();
    }

    private static void AppendSegment(StringBuilder sb, string segment)
    {
        if (segment.Length == 0)
        {
            return;
        }

        if (sb.Length > 0)
        {
            sb.Append(' ');
        }

        sb.Append(segment);
    }

    private static string StripInlineComment(string line)
    {
        var markerIndex = line.IndexOf('!');
        var withoutComment = markerIndex >= 0 ? line[..markerIndex] : line;
        return withoutComment.Trim();
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToLowerInvariant();
    }
}

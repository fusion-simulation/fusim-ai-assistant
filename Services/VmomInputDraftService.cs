using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public sealed record VmomInputDraft(
    IReadOnlyDictionary<string, string> Fields,
    string InputContent,
    IReadOnlyList<string> RejectedKeys);

public sealed class VmomInputDraftService
{
    private readonly VmomNamelistBuilder _namelistBuilder;

    public VmomInputDraftService(VmomNamelistBuilder namelistBuilder)
    {
        _namelistBuilder = namelistBuilder;
    }

    public VmomInputDraft ParseEqinpt(string inputContent)
    {
        var fields = ParseEqinptFields(inputContent ?? string.Empty);
        return new VmomInputDraft(
            fields,
            _namelistBuilder.BuildEqinptNamelist(fields),
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
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        var inEqinpt = false;
        string? currentKey = null;
        var currentValue = new List<string>();

        foreach (var rawLine in lines)
        {
            var line = StripInlineComment(rawLine);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!inEqinpt)
            {
                if (line.StartsWith("&eqinpt", StringComparison.OrdinalIgnoreCase))
                {
                    inEqinpt = true;
                }

                continue;
            }

            if (line == "/")
            {
                FlushCurrent();
                break;
            }

            var eqIndex = line.IndexOf('=');
            if (eqIndex >= 0)
            {
                FlushCurrent();

                currentKey = NormalizeKey(line[..eqIndex]);
                currentValue.Add(line[(eqIndex + 1)..].Trim());
            }
            else if (!string.IsNullOrWhiteSpace(currentKey))
            {
                currentValue.Add(line);
            }
        }

        FlushCurrent();
        return result;

        static string StripInlineComment(string line)
        {
            var markerIndex = line.IndexOf('!');
            var withoutComment = markerIndex >= 0 ? line[..markerIndex] : line;
            return withoutComment.Trim();
        }

        void FlushCurrent()
        {
            if (string.IsNullOrWhiteSpace(currentKey) || currentValue.Count == 0)
            {
                currentKey = null;
                currentValue.Clear();
                return;
            }

            var merged = string.Join(" ", currentValue)
                .Trim()
                .TrimEnd(',')
                .Trim();

            result[currentKey] = merged;
            currentKey = null;
            currentValue.Clear();
        }
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToLowerInvariant();
    }
}

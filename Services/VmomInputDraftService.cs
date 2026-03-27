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
        var fields = new Dictionary<string, string>(draft.Fields, StringComparer.Ordinal);
        var rejectedKeys = new List<string>();

        foreach (var change in changes ?? Array.Empty<SubmitAgentProposedChange>())
        {
            if (string.IsNullOrWhiteSpace(change.FieldKey))
            {
                continue;
            }

            var key = change.FieldKey.Trim();
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
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in fields)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key.Trim()] = value?.Trim() ?? string.Empty;
        }

        return result;
    }

    private static Dictionary<string, string> ParseEqinptFields(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        var inEqinpt = false;
        string? currentKey = null;
        var currentValue = new List<string>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('!'))
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

                currentKey = line[..eqIndex].Trim();
                currentValue.Add(line[(eqIndex + 1)..].Trim());
            }
            else if (!string.IsNullOrWhiteSpace(currentKey))
            {
                currentValue.Add(line);
            }
        }

        FlushCurrent();
        return result;

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
}

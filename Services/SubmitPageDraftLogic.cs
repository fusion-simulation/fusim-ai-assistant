using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public static class SubmitPageDraftLogic
{
    public static bool CanSwitchToFormMode(string inputContent, VmomInputDraft draft)
    {
        return string.IsNullOrWhiteSpace(inputContent) || draft.Fields.Count > 0;
    }

    public static Dictionary<string, string> BuildFormSubmissionFields(
        IReadOnlyDictionary<string, string> draftFields)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in draftFields)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key.Trim()] = value ?? string.Empty;
        }

        return result;
    }

    public static int CountAppliedChanges(
        VmomInputDraft currentDraft,
        VmomInputDraft updatedDraft,
        IReadOnlyList<SubmitAgentProposedChange> changes)
    {
        var appliedCount = 0;

        foreach (var change in changes)
        {
            if (string.IsNullOrWhiteSpace(change.FieldKey) ||
                string.IsNullOrWhiteSpace(change.SuggestedValue) ||
                !currentDraft.Fields.ContainsKey(change.FieldKey))
            {
                continue;
            }

            if (updatedDraft.Fields.TryGetValue(change.FieldKey, out var updatedValue) &&
                string.Equals(updatedValue, change.SuggestedValue.Trim(), StringComparison.Ordinal))
            {
                appliedCount++;
            }
        }

        return appliedCount;
    }
}

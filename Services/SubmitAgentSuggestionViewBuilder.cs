using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public static class SubmitAgentSuggestionViewBuilder
{
    public static IReadOnlyList<SubmitAgentSuggestionItem> Build(
        IReadOnlyList<SubmitAgentProposedChange>? changes)
    {
        return (changes ?? Array.Empty<SubmitAgentProposedChange>())
            .Select((change, index) => new SubmitAgentSuggestionItem(
                $"{change.FieldKey}:{index}",
                change.FieldKey,
                change.CurrentValue,
                change.SuggestedValue,
                change.Reason,
                true))
            .ToList();
    }
}

public sealed record SubmitAgentSuggestionItem(
    string Id,
    string FieldKey,
    string CurrentValue,
    string SuggestedValue,
    string Reason,
    bool IsSelected);

using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public static class CaseDetailChatViewBuilder
{
    public static IReadOnlyList<CaseDetailChatDisplayItem> Build(
        IReadOnlyList<CaseAgentChatMessage>? history,
        bool isChatBusy)
    {
        var items = (history ?? Array.Empty<CaseAgentChatMessage>())
            .Select(message => new CaseDetailChatDisplayItem(
                string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase),
                false,
                message.Content,
                message.ImageUrl))
            .ToList();

        if (isChatBusy)
        {
            items.Add(new CaseDetailChatDisplayItem(
                false,
                true,
                "正在分析...",
                null));
        }

        return items;
    }
}

public sealed record CaseDetailChatDisplayItem(
    bool IsUser,
    bool IsWaiting,
    string Content,
    string? ImageUrl);

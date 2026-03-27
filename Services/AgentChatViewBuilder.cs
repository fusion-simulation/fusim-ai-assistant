using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public static class AgentChatViewBuilder
{
    public static IReadOnlyList<AgentChatDisplayItem> Build(
        IReadOnlyList<CaseAgentChatMessage>? history,
        bool isChatBusy)
    {
        var items = (history ?? Array.Empty<CaseAgentChatMessage>())
            .Select(message => new AgentChatDisplayItem(
                string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase),
                false,
                message.Content,
                message.ImageUrl))
            .ToList();

        if (isChatBusy)
        {
            items.Add(new AgentChatDisplayItem(
                false,
                true,
                "正在分析...",
                null));
        }

        return items;
    }
}

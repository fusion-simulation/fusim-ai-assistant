using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public static class CaseDetailChatViewBuilder
{
    public static IReadOnlyList<AgentChatDisplayItem> Build(
        IReadOnlyList<CaseAgentChatMessage>? history,
        bool isChatBusy)
    {
        return AgentChatViewBuilder.Build(history, isChatBusy);
    }
}

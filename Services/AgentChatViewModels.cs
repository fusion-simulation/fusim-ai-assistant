namespace FusimAiAssiant.Services;

public sealed record AgentChatDisplayItem(
    bool IsUser,
    bool IsWaiting,
    string Content,
    string? ImageUrl);

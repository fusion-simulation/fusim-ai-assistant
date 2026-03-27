namespace FusimAiAssiant.Services;

public static class SubmitAgentShortcutCatalog
{
    public static IReadOnlyList<string> DefaultShortcuts { get; } =
    [
        "检查当前参数是否存在明显冲突",
        "给出更保守的收敛性调参建议",
        "帮我检查剖面参数是否足够平滑",
        "如果我想先提高稳定性，应该优先改哪些参数"
    ];

    public static string ApplyShortcut(string currentDraft, string shortcut)
    {
        if (string.IsNullOrWhiteSpace(shortcut))
        {
            return currentDraft;
        }

        if (string.IsNullOrWhiteSpace(currentDraft))
        {
            return shortcut;
        }

        return currentDraft.TrimEnd() + Environment.NewLine + shortcut;
    }
}

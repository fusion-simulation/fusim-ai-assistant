namespace FusimAiAssiant.Services;

public static class CaseAgentShortcutCatalog
{
    public static IReadOnlyList<string> DefaultShortcuts { get; } =
    [
        "总结当前算例结果",
        "分析当前算例是否收敛",
        "解释 vmom.out 里的异常或警告",
        "绘制 eqpr_iota.txt 中 x=r y=q 的曲线"
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

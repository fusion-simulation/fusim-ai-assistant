using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class CaseAgentShortcutCatalogTests
{
    [Fact]
    public void DefaultShortcuts_ReturnExpectedPrompts()
    {
        var shortcuts = CaseAgentShortcutCatalog.DefaultShortcuts;

        Assert.Collection(
            shortcuts,
            item => Assert.Equal("总结当前算例结果", item),
            item => Assert.Equal("分析当前算例是否收敛", item),
            item => Assert.Equal("解释 vmom.out 里的异常或警告", item),
            item => Assert.Equal("绘制 eqpr_iota.txt 中 x=r y=q 的曲线", item));
    }

    [Fact]
    public void ApplyShortcut_ReturnsShortcut_WhenDraftIsEmpty()
    {
        var result = CaseAgentShortcutCatalog.ApplyShortcut(string.Empty, "总结当前算例结果");

        Assert.Equal("总结当前算例结果", result);
    }

    [Fact]
    public void ApplyShortcut_AppendsToExistingDraft()
    {
        var result = CaseAgentShortcutCatalog.ApplyShortcut("旧的草稿内容", "分析当前算例是否收敛");

        Assert.Equal("旧的草稿内容" + Environment.NewLine + "分析当前算例是否收敛", result);
    }
}

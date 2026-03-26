using System.Reflection;
using FusimAiAssiant.Models;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class CaseDetailChatViewBuilderTests
{
    [Fact]
    public void Build_PreservesMessagesAndAppendsWaitingPlaceholder_WhenBusy()
    {
        var builderType = Type.GetType("FusimAiAssiant.Services.CaseDetailChatViewBuilder, FusimAiAssiant");
        Assert.NotNull(builderType);

        var buildMethod = builderType!.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(buildMethod);

        var history = new[]
        {
            new CaseAgentChatMessage("user", "请分析 RZ.txt 的异常", null),
            new CaseAgentChatMessage("assistant", "我先检查一下关键参数。", null)
        };

        var result = buildMethod!.Invoke(null, new object?[] { history, true });
        var items = Assert.IsAssignableFrom<IReadOnlyList<object>>(result);

        Assert.Equal(3, items.Count);
        Assert.Equal("请分析 RZ.txt 的异常", GetString(items[0], "Content"));
        Assert.Equal("我先检查一下关键参数。", GetString(items[1], "Content"));
        Assert.True(GetBool(items[2], "IsWaiting"));
        Assert.False(GetBool(items[2], "IsUser"));
        Assert.Equal("正在分析...", GetString(items[2], "Content"));
    }

    [Fact]
    public void Build_DoesNotAppendWaitingPlaceholder_WhenNotBusy()
    {
        var builderType = Type.GetType("FusimAiAssiant.Services.CaseDetailChatViewBuilder, FusimAiAssiant");
        Assert.NotNull(builderType);

        var buildMethod = builderType!.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(buildMethod);

        var history = new[]
        {
            new CaseAgentChatMessage("user", "看一下 vmom.out", null)
        };

        var result = buildMethod!.Invoke(null, new object?[] { history, false });
        var items = Assert.IsAssignableFrom<IReadOnlyList<object>>(result);

        Assert.Single(items);
        Assert.False(GetBool(items[0], "IsWaiting"));
        Assert.Equal("看一下 vmom.out", GetString(items[0], "Content"));
    }

    private static string GetString(object item, string propertyName)
    {
        var value = item.GetType().GetProperty(propertyName)?.GetValue(item);
        return Assert.IsType<string>(value);
    }

    private static bool GetBool(object item, string propertyName)
    {
        var value = item.GetType().GetProperty(propertyName)?.GetValue(item);
        return Assert.IsType<bool>(value);
    }
}

using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class AgentChatViewBuilderTests
{
    [Fact]
    public void Build_AddsWaitingAssistantRow_WhenBusy()
    {
        var history = new[]
        {
            new CaseAgentChatMessage("user", "请分析一下", null),
            new CaseAgentChatMessage("assistant", "我先看看。", null)
        };

        var items = AgentChatViewBuilder.Build(history, true);

        Assert.Equal(3, items.Count);
        Assert.True(items[^1].IsWaiting);
        Assert.False(items[^1].IsUser);
        Assert.Equal("正在分析...", items[^1].Content);
    }
}

using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class ClientThemeServiceTests
{
    [Fact]
    public void SetTheme_UpdatesCurrentTheme_AndRaisesEventOnce()
    {
        var service = new ClientThemeService();
        var raisedCount = 0;
        service.ThemeChanged += () => raisedCount++;

        service.SetTheme(ThemeMode.Light);

        Assert.Equal(ThemeMode.Light, service.CurrentTheme);
        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void SetTheme_DoesNotRaiseEvent_WhenThemeIsUnchanged()
    {
        var service = new ClientThemeService();
        var raisedCount = 0;
        service.ThemeChanged += () => raisedCount++;

        service.SetTheme(ThemeMode.Dark);

        Assert.Equal(ThemeMode.Dark, service.CurrentTheme);
        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public void Toggle_SwitchesTheme_AndRaisesEvent()
    {
        var service = new ClientThemeService();
        var raisedCount = 0;
        service.ThemeChanged += () => raisedCount++;

        service.Toggle();

        Assert.Equal(ThemeMode.Light, service.CurrentTheme);
        Assert.Equal(1, raisedCount);
    }
}

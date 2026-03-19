using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class ThemeModeTests
{
    [Fact]
    public void Default_UsesDarkTheme()
    {
        var mode = ThemeMode.Default;

        Assert.Equal(ThemeMode.Dark, mode);
        Assert.Equal("dark", mode.ToAttributeValue());
    }

    [Theory]
    [InlineData("dark", "dark")]
    [InlineData("light", "light")]
    [InlineData("LIGHT", "light")]
    [InlineData("", "dark")]
    [InlineData(null, "dark")]
    [InlineData("unknown", "dark")]
    public void FromAttributeValue_ReturnsExpectedTheme(string? attributeValue, string expectedAttributeValue)
    {
        var mode = ThemeMode.FromAttributeValue(attributeValue);

        Assert.Equal(expectedAttributeValue, mode.ToAttributeValue());
    }

    [Fact]
    public void Toggle_SwitchesBetweenDarkAndLight()
    {
        var firstToggle = ThemeMode.Dark.Toggle();
        var secondToggle = firstToggle.Toggle();

        Assert.Equal(ThemeMode.Light, firstToggle);
        Assert.Equal("light", firstToggle.ToAttributeValue());
        Assert.Equal(ThemeMode.Dark, secondToggle);
    }
}

namespace FusimAiAssiant.Services;

public sealed class ClientThemeService
{
    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Default;

    public event Action? ThemeChanged;

    public void SetTheme(ThemeMode theme)
    {
        if (theme == CurrentTheme)
        {
            return;
        }

        CurrentTheme = theme;
        ThemeChanged?.Invoke();
    }

    public void Toggle()
    {
        SetTheme(CurrentTheme.Toggle());
    }
}

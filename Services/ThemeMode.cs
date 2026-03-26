namespace FusimAiAssiant.Services;

public readonly record struct ThemeMode(string Value)
{
    public static ThemeMode Default => Dark;

    public static ThemeMode Dark => new("dark");

    public static ThemeMode Light => new("light");

    public ThemeMode Toggle() => Value == Light.Value ? Dark : Light;

    public string ToAttributeValue() => Value;

    public static ThemeMode FromAttributeValue(string? attributeValue)
    {
        if (string.Equals(attributeValue, Light.Value, StringComparison.OrdinalIgnoreCase))
        {
            return Light;
        }

        return Dark;
    }

    public override string ToString() => Value;
}

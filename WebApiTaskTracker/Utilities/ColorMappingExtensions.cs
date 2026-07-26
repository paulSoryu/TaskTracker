namespace WebApiTaskTracker.Utilities;

public static class ColorMappingExtensions
{
    public static string ToHexColor(this int argb)
    {
        return "#" + (argb & 0xFFFFFFFF).ToString("X8");
    }

    public static int ToArgbColor(this string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return 0;

        string cleanHex = hex.Replace("#", "").Trim();

        if (cleanHex.Length == 6)
        {
            cleanHex = "FF" + cleanHex;
        }

        return unchecked((int)uint.Parse(cleanHex, System.Globalization.NumberStyles.HexNumber));
    }
}

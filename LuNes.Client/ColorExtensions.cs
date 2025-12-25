using Raylib_cs;

namespace LuNes.Client;

public static class ColorExtensions
{
    public static bool EqualsEx(this Color color, Color other)
    {
        return color.R == other.R && color.G == other.G && color.B == other.B && color.A == other.A;
    }
    
    public static int ToInteger(this Color color)
    {
        return (color.R << 24) | (color.G << 16) | (color.B << 8) | color.A;
    }
}
namespace LuNes;

public static class Helpers
{
    public static string Hex(uint n, byte d)
    {
        char[] s = new string('0', d).ToCharArray();
        for (int i = d - 1; i >= 0; i--, n >>= 4)
            s[i] = "0123456789ABCDEF"[(int)(n & 0xF)];
        return new string(s);
    }
}
namespace LuNes.Cpu;

public static class Cpu6502Extensions
{
    public static byte ToByte(this int value) => (byte)(value & 0xFF);
    public static byte ToByte(this uint value) => (byte)((value >> 8) & 0xFF);
    public static ushort ToUshort(this int value) => (ushort)(value & 0xFFFF);
    public static ushort ToUshort(this uint value) => (ushort)(value & 0xFFFF);

    public static byte WrapAdd(this byte a, byte b) => (byte)((a + b) & 0xFF);
    public static ushort WrapAdd(this ushort a, ushort b) => (ushort)((a + b) & 0xFFFF);

    public static byte WrapSub(this byte a, byte b) => (byte)((a - b) & 0xFF);
    public static ushort WrapSub(this ushort a, ushort b) => (ushort)((a - b) & 0xFFFF);
    
    public static byte WrapSubWithBorrow(this byte a, byte b, bool borrow) 
        => (byte)((a - b - (borrow ? 1 : 0)) & 0xFF);
    
    public static byte ToByte(this sbyte value) => (byte) value;
    public static sbyte ToSignedByte(this byte value) => (sbyte) value;
}
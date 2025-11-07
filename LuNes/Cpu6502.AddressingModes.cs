namespace LuNes;

public partial class Cpu6502
{
    /// <summary>
    /// Implied
    /// </summary>
    public byte Imp()
    {
        Fetched = A;
        return 0;
    }

    /// <summary>
    /// Immediate
    /// </summary>
    public byte Imm()
    {
        AddressAbsolute = Pc++;
        return 0;
    }

    public byte Zp0()
    {
        AddressAbsolute = Read(Pc);
        Pc++;
        AddressAbsolute &= 0x00FF;
        return 0;
    }

    public byte Zpx()
    {
        AddressAbsolute = (ushort)(Read(Pc) + X);
        Pc++;
        AddressAbsolute &= 0x00FF;
        return 0;
    }

    public byte Zpy()
    {
        AddressAbsolute = (ushort)(Read(Pc) + Y);
        Pc++;
        AddressAbsolute &= 0x00FF;
        return 0;
    }

    public byte Rel()
    {
        AddressRelative = Read(Pc);
        Pc++;
        if ((AddressRelative & 0x80) != 0)
            AddressRelative = (ushort)(AddressRelative | 0xFF00);

        return 0;
    }

    public byte Abs()
    {
        var lo = Read(Pc);
        Pc++;
        var hi = Read(Pc);
        Pc++;

        AddressAbsolute = (ushort)((hi << 8) | lo);

        return 0;
    }

    public byte Abx()
    {
        var lo = Read(Pc);
        Pc++;
        var hi = Read(Pc);
        Pc++;

        AddressAbsolute = (ushort)((hi << 8) | lo);
        AddressAbsolute += X;

        if ((AddressAbsolute & 0xFF00) != (hi << 8))
            return 1;
        
        return 0;
    }

    public byte Aby()
    {
        var lo = Read(Pc);
        Pc++;
        var hi = Read(Pc);
        Pc++;

        AddressAbsolute = (ushort)((hi << 8) | lo);
        AddressAbsolute += Y;

        if ((AddressAbsolute & 0xFF00) != (hi << 8))
            return 1;
        
        return 0;
    }

    public byte Ind()
    {
        var ptrLo = Read(Pc);
        Pc++;
        var ptrHi = Read(Pc);
        Pc++;

        var ptr = (ushort)((ptrHi << 8) | ptrLo);

        if (ptrLo == 0x00FF) // Simulate page boundary hardware bug
            AddressAbsolute = (ushort)((Read((ptr & 0xFF00)) << 8) | Read(ptr));
        else
            AddressAbsolute = (ushort)((Read((ptr + 1)) << 8) | Read(ptr));

        return 0;
    }

    public byte Izx()
    {
        var t = Read(Pc);
        Pc++;

        var lo = Read((ushort)((ushort)(t + X) & 0x00FF));
        var hi = Read((ushort)((ushort)(t + X + 1) & 0x00FF));

        AddressAbsolute = (ushort)((hi << 8) | lo);

        return 0;
    }

    public byte Izy()
    {
        var t = Read(Pc);
        Pc++;

        var lo = Read((ushort)(t & 0x00FF));
        var hi = Read((ushort)((t + 1) & 0x00FF));

        AddressAbsolute = (ushort)((hi << 8) | lo);
        AddressAbsolute += Y;

        if ((AddressAbsolute & 0xFF00) != (hi << 8))
            return 1;

        return 0;
    }
}
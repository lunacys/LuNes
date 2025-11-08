namespace LuNes;

public partial class Cpu6502
{
    /// <summary>
    /// Implied.
    /// There is no additional data required for this instruction. The instruction
    /// does something very simple like like sets a status bit. However, we will
    /// target the accumulator, for instructions like PHA
    /// </summary>
    public byte Imp()
    {
        Fetched = A;
        return 0;
    }

    /// <summary>
    /// Immediate.
    /// The instruction expects the next byte to be used as a value, so we'll prep
    /// the read address to point to the next byte
    /// </summary>
    public byte Imm()
    {
        AddressAbsolute = Pc++;
        return 0;
    }

    /// <summary>
    /// Zero Page.
    /// To save program bytes, zero page addressing allows you to absolutely address
    /// a location in first 0xFF bytes of address range. Clearly this only requires
    /// one byte instead of the usual two.
    /// </summary>
    /// <returns></returns>
    public byte Zp0()
    {
        AddressAbsolute = Read(Pc);
        Pc++;
        AddressAbsolute &= 0x00FF;
        return 0;
    }

    /// <summary>
    /// Zero Page with X Offset.
    /// </summary>
    /// <returns></returns>
    public byte Zpx()
    {
        AddressAbsolute = (ushort)(Read(Pc) + X);
        Pc++;
        AddressAbsolute &= 0x00FF;
        return 0;
    }

    /// <summary>
    /// Zero Page with Y Offset.
    /// </summary>
    /// <returns></returns>
    public byte Zpy()
    {
        AddressAbsolute = (ushort)(Read(Pc) + Y);
        Pc++;
        AddressAbsolute &= 0x00FF;
        return 0;
    }

    /// <summary>
    /// Relative.
    /// This address mode is exclusive to branch instructions. The address
    /// must reside within -128 to +127 of the branch instruction, i.e.
    /// you cant directly branch to any address in the addressable range.
    /// </summary>
    /// <returns></returns>
    public byte Rel()
    {
        AddressRelative = Read(Pc);
        Pc++;
        if ((AddressRelative & 0x80) != 0)
            AddressRelative = (ushort)(AddressRelative | 0xFF00);

        return 0;
    }

    /// <summary>
    /// Absolute 
    /// A full 16-bit address is loaded and used
    /// </summary>
    /// <returns></returns>
    public byte Abs()
    {
        var lo = Read(Pc);
        Pc++;
        var hi = Read(Pc);
        Pc++;

        AddressAbsolute = (ushort)((hi << 8) | lo);

        return 0;
    }

    /// <summary>
    /// Absolute with X Offset.
    /// Fundamentally the same as absolute addressing, but the contents of the X Register
    /// is added to the supplied two byte address. If the resulting address changes
    /// the page, an additional clock cycle is required
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Absolute with Y Offset.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Indirect.
    /// The supplied 16-bit address is read to get the actual 16-bit address. This is
    /// instruction is unusual in that it has a bug in the hardware! To emulate its
    /// function accurately, we also need to emulate this bug. If the low byte of the
    /// supplied address is 0xFF, then to read the high byte of the actual address
    /// we need to cross a page boundary. This doesn't actually work on the chip as 
    /// designed, instead it wraps back around in the same page, yielding an 
    /// invalid actual address
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Indirect X.
    /// The supplied 8-bit address is offset by X Register to index
    /// a location in page 0x00. The actual 16-bit address is read 
    /// from this location
    /// </summary>
    /// <returns></returns>
    public byte Izx()
    {
        var t = Read(Pc);
        Pc++;

        var lo = Read((ushort)((ushort)(t + X) & 0x00FF));
        var hi = Read((ushort)((ushort)(t + X + 1) & 0x00FF));

        AddressAbsolute = (ushort)((hi << 8) | lo);

        return 0;
    }

    /// <summary>
    /// Indirect Y.
    /// </summary>
    /// <returns></returns>
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
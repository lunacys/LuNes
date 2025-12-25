namespace LuNes.Cpu;

public partial class Cpu6502
{
    /// <summary>
    /// Branch Always (W65C02)
    /// </summary>
    public byte Bra()
    {
        // Always branch
        Cycles++;
        AddressAbsolute = (ushort)(Pc + AddressRelative);
    
        if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
            Cycles++;
    
        Pc = AddressAbsolute;
        return 0;
    }

    /// <summary>
    /// Store Zero (W65C02)
    /// </summary>
    public byte Stz()
    {
        Write(AddressAbsolute, 0x00);
        return 0;
    }

    /// <summary>
    /// Push X Register (W65C02)
    /// </summary>
    public byte Phx()
    {
        Write((ushort)(0x0100 + Stkp), X);
        Stkp--;
        return 0;
    }

    /// <summary>
    /// Pull X Register (W65C02)
    /// </summary>
    public byte Plx()
    {
        Stkp++;
        X = Read((ushort)(0x0100 + Stkp));
        SetFlag(Flags.Zero, X == 0x00);
        SetFlag(Flags.Negative, (X & 0x80) != 0);
        return 0;
    }

    /// <summary>
    /// Push Y Register (W65C02)
    /// </summary>
    public byte Phy()
    {
        Write((ushort)(0x0100 + Stkp), Y);
        Stkp--;
        return 0;
    }

    /// <summary>
    /// Pull Y Register (W65C02)
    /// </summary>
    public byte Ply()
    {
        Stkp++;
        Y = Read((ushort)(0x0100 + Stkp));
        SetFlag(Flags.Zero, Y == 0x00);
        SetFlag(Flags.Negative, (Y & 0x80) != 0);
        return 0;
    }

    /// <summary>
    /// Wait for Interrupt (W65C02)
    /// </summary>
    public byte Wai()
    {
        // Set a flag or wait for interrupt
        // In an emulator, you might pause execution
        // until an interrupt occurs
        _isWaitingForInterrupt = true;
        return 0;
    }

    /// <summary>
    /// Stop (W65C02) - Stops the clock
    /// </summary>
    public byte Stp()
    {
        // Halt the CPU
        _isStopped = true;
        return 0;
    }

    /// <summary>
    /// BIT Immediate (W65C02)
    /// </summary>
    public byte BitImm()
    {
        Fetch();
        ushort temp = (ushort)(A & Fetched);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x00);
        SetFlag(Flags.Negative, (Fetched & (1 << 7)) != 0);
        SetFlag(Flags.Overflow, (Fetched & (1 << 6)) != 0);
        return 0;
    }

    private bool _isWaitingForInterrupt = false;
    private bool _isStopped = false;
}
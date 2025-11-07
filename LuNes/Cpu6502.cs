namespace LuNes;

public partial class Cpu6502
{
    [Flags]
    public enum Flags : byte
    {
        None = 0,
        /// <summary>
        /// C
        /// </summary>
        CarryBit = 1 << 0,
        /// <summary>
        /// Z
        /// </summary>
        Zero = 1 << 1,
        /// <summary>
        /// I
        /// </summary>
        DisableInterrupts = 1 << 2,
        /// <summary>
        /// D
        /// </summary>
        DecimalMode = 1 << 3,
        /// <summary>
        /// B
        /// </summary>
        Break = 1 << 4,
        /// <summary>
        /// U
        /// </summary>
        Unused = 1 << 5,
        /// <summary>
        /// V
        /// </summary>
        Overflow = 1 << 6,
        /// <summary>
        /// N
        /// </summary>
        Negative = 1 << 7,
    }
    
    private Bus? _bus;

    /// <summary>
    /// Status Register
    /// </summary>
    public Flags Status = Flags.None;
    /// <summary>
    /// Accumulator Register
    /// </summary>
    public byte A = 0;
    /// <summary>
    /// X Register
    /// </summary>
    public byte X = 0;
    /// <summary>
    /// Y Register
    /// </summary>
    public byte Y = 0;
    /// <summary>
    /// Stack Pointer. Points to location on bus
    /// </summary>
    public byte Stkp = 0;
    /// <summary>
    /// Program Counter
    /// </summary>
    public ushort Pc = 0x0000;

    public byte Fetched = 0;
    public ushort AddressAbsolute = 0;
    public ushort AddressRelative = 0;
    public byte Opcode = 0;
    public byte Cycles = 0;

    public int ClockCount = 0;

    public void ConnectBus(Bus bus) => _bus = bus;

    public byte Read(ushort address) => _bus?.Read(address) ?? 0;
    public byte Read(int address) => Read((ushort)address);

    public void Write(ushort address, byte data) => _bus?.Write(address, data);
    public void Write(int address, byte data) => Write((ushort)address, data);
    public void Write(ushort address, int data) => Write(address, data);
    public void Write(int address, int data) => Write((ushort)address, data);

    public void Clock()
    {
        if (Cycles == 0)
        {
            Opcode = Read(Pc);
            
            SetFlag(Flags.Unused, 1);
            
            Pc++;

            Cycles = Lookup[Opcode].Cycles;

            var additionalCycle1 = Lookup[Opcode].AddressMode();
            var additionalCycle2 = Lookup[Opcode].Operation();

            Cycles += (byte)(additionalCycle1 & additionalCycle2);
            
            SetFlag(Flags.Unused, 1);
        }

        ClockCount++;
        Cycles--;
    }

    public void Reset()
    {
        AddressAbsolute = 0xFFFC;
        var lo = Read(AddressAbsolute);
        var hi = Read((ushort)(AddressAbsolute + 1));

        Pc = (ushort)((hi << 8) | lo);

        A = 0;
        X = 0;
        Y = 0;
        Stkp = 0xFD;
        Status = 0x00 | Flags.Unused;

        AddressRelative = 0;
        AddressAbsolute = 0;
        Fetched = 0;

        Cycles = 8;
    }

    /// <summary>
    /// Interrupt request
    /// </summary>
    public void Irq()
    {
        // If interrupts are allowed
        if (GetFlag(Flags.DisableInterrupts) == 0)
        {
            // Push the program counter to the stack. It's 16-bits don't
            // forget so that takes two pushes
            Write((ushort)(0x0100 + Stkp), (byte)((Pc >> 8) & 0x00FF));
            Stkp--;
            Write((ushort)(0x0100 + Stkp), (byte)(Pc & 0x00FF));
            Stkp--;
            
            // Then Push the status register to the stack
            SetFlag(Flags.Break, 0);
            SetFlag(Flags.Unused, 1);
            SetFlag(Flags.DisableInterrupts, 1);
            Write((ushort)(0x0100 + Stkp), (byte)Status);
            Stkp--;

            // Read new program counter location from fixed address
            AddressAbsolute = 0xFFFE;
            var lo = Read(AddressAbsolute);
            var hi = Read((ushort)(AddressAbsolute + 1));
            Pc = (ushort)((hi << 8) | lo);

            Cycles = 7;
        }
    }

    /// <summary>
    /// Non-maskable interrupt request. Can never be disabled.
    /// </summary>
    public void Nmi()
    {
        Write((ushort)(0x0100 + Stkp), (byte)((Pc >> 8) & 0x00FF));
        Stkp--;
        Write((ushort)(0x0100 + Stkp), (byte)(Pc & 0x00FF));
        Stkp--;
        
        SetFlag(Flags.Break, 0);
        SetFlag(Flags.Unused, 1);
        SetFlag(Flags.DisableInterrupts, 1);
        Write((ushort)(0x0100 + Stkp), (byte)Status);
        Stkp--;

        AddressAbsolute = 0xFFFA;
        var lo = Read(AddressAbsolute);
        var hi = Read((ushort)(AddressAbsolute + 1));
        Pc = (ushort)((hi << 8) | lo);

        Cycles = 8;
    }

    public byte Fetch()
    {
        if (Lookup[Opcode].AddressMode != Imp)
            Fetched = Read(AddressAbsolute);
        
        return Fetched;
    }

    public bool IsComplete() => Cycles == 0;

    public Dictionary<ushort, string> Disassemble(ushort start, ushort stop)
{
    var mapLines = new Dictionary<ushort, string>();
    int addr = start;
    byte value = 0x00, lo = 0x00, hi = 0x00;
    ushort lineAddr = 0;

    string Hex(uint n, byte d)
    {
        char[] s = new string('0', d).ToCharArray();
        for (int i = d - 1; i >= 0; i--, n >>= 4)
            s[i] = "0123456789ABCDEF"[(int)(n & 0xF)];
        return new string(s);
    }

    while (addr <= stop)
    {
        lineAddr = (ushort)addr;

        var sInst = $"${Hex((uint)addr, 4)}: ";

        byte opcode = _bus?.Read(addr, true) ?? 0;
        addr++;
        sInst += Lookup[opcode].Name + " ";

        var addrMode = Lookup[opcode].AddressMode;
        if (addrMode == Imp)
        {
            sInst += " {IMP}";
        }
        else if (addrMode == Imm)
        {
            value = _bus?.Read(addr, true) ?? 0;
            addr++;
            sInst += "#$" + Hex(value, 2) + " {IMM}";
        }
        else if (addrMode == Zp0)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = 0x00;
            sInst += "$" + Hex(lo, 2) + " {ZP0}";
        }
        else if (addrMode == Zpx)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = 0x00;
            sInst += "$" + Hex(lo, 2) + ", X {ZPX}";
        }
        else if (addrMode == Zpy)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = 0x00;
            sInst += "$" + Hex(lo, 2) + ", Y {ZPY}";
        }
        else if (addrMode == Izx)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = 0x00;
            sInst += "($" + Hex(lo, 2) + ", X) {IZX}";
        }
        else if (addrMode == Izy)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = 0x00;
            sInst += "($" + Hex(lo, 2) + "), Y {IZY}";
        }
        else if (addrMode == Abs)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = _bus?.Read(addr, true) ?? 0;
            addr++;
            sInst += "$" + Hex((ushort)((hi << 8) | lo), 4) + " {ABS}";
        }
        else if (addrMode == Abx)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = _bus?.Read(addr, true) ?? 0;
            addr++;
            sInst += "$" + Hex((ushort)((hi << 8) | lo), 4) + ", X {ABX}";
        }
        else if (addrMode == Aby)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = _bus?.Read(addr, true) ?? 0;
            addr++;
            sInst += "$" + Hex((ushort)((hi << 8) | lo), 4) + ", Y {ABY}";
        }
        else if (addrMode == Ind)
        {
            lo = _bus?.Read(addr, true) ?? 0;
            addr++;
            hi = _bus?.Read(addr, true) ?? 0;
            addr++;
            sInst += "($" + Hex((ushort)((hi << 8) | lo), 4) + ") {IND}";
        }
        else if (addrMode == Rel)
        {
            value = _bus?.Read(addr, true) ?? 0;
            addr++;
            // Handle relative address calculation (accounting for signed byte)
            short offset = (short)(sbyte)value; // Sign-extend the byte
            ushort targetAddr = (ushort)(addr + offset);
            sInst += "$" + Hex(value, 2) + " [$" + Hex(targetAddr, 4) + "] {REL}";
        }

        mapLines[lineAddr] = sInst;
    }

    return mapLines;
}

    private byte GetFlag(Flags flag)
    {
        return (byte)(((Status & flag) > 0) ? 1 : 0);
    }

    private void SetFlag(Flags flag, byte value)
    {
        if (value != 0)
            Status |= flag;
        else 
            Status &= ~flag;
    }

    private void SetFlag(Flags flag, bool value)
    {
        if (value)
            Status |= flag;
        else 
            Status &= ~flag;
    }

    private void SetFlag(Flags flag, int value) => SetFlag(flag, (byte)value);
    private void SetFlag(Flags flag, ushort value) => SetFlag(flag, (byte)value);


}
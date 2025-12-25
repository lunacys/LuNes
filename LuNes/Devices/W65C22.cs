namespace LuNes.Devices;

public class W65C22 : MemoryMappedDevice
{
    // Registers
    private byte[] _registers = new byte[16];
    public byte PortA { get; private set; }      // Data Register A
    public byte PortB { get; private set; }      // Data Register B
    public byte DdrA { get; private set; }       // Data Direction Register A
    public byte DdrB { get; private set; }       // Data Direction Register B
    
    // Timers (simplified)
    private ushort _timer1Counter;
    private ushort _timer1Latch;
    private bool _timer1Running;
    
    // Interrupt flags
    private byte _interruptFlag;
    private byte _interruptEnable;
    
    public W65C22(ushort baseAddress) : base(baseAddress, (ushort)(baseAddress + 0x0F))
    {
        Reset();
    }
    
    public void Reset()
    {
        PortA = 0;
        PortB = 0;
        DdrA = 0;
        DdrB = 0;
        _timer1Counter = 0;
        _timer1Latch = 0xFFFF;
        _timer1Running = false;
        _interruptFlag = 0;
        _interruptEnable = 0;
    }
    
    public override void Clock()
    {
        // Update timers
        if (_timer1Running && _timer1Counter > 0)
        {
            _timer1Counter--;
            if (_timer1Counter == 0)
            {
                // Timer expired
                _interruptFlag |= 0x40; // Timer 1 interrupt flag
                // Reload if in continuous mode
                if ((_registers[0x0B] & 0x40) == 0) // Check T1 control bit 6
                {
                    _timer1Counter = _timer1Latch;
                }
            }
        }
    }
    
    protected override byte OnRead(ushort address, bool isReadOnly)
    {
        byte reg = (byte)(address - StartAddress);
        
        switch (reg)
        {
            case 0x00: // Port B
                return ReadPortB();
            case 0x01: // Port A
                return ReadPortA();
            case 0x02: // DDRB
                return DdrB;
            case 0x03: // DDRA
                return DdrA;
            case 0x04: // Timer 1 Low
                return (byte)(_timer1Counter & 0xFF);
            case 0x05: // Timer 1 High
                return (byte)(_timer1Counter >> 8);
            case 0x06: // Timer 1 Latch Low
                return (byte)(_timer1Latch & 0xFF);
            case 0x07: // Timer 1 Latch High
                return (byte)(_timer1Latch >> 8);
            case 0x08: // Timer 2 Low
                return 0; // Simplified
            case 0x09: // Timer 2 High
                return 0;
            case 0x0A: // Shift Register
                return 0;
            case 0x0B: // ACR (Auxiliary Control Register)
                return _registers[0x0B];
            case 0x0C: // PCR (Peripheral Control Register)
                return _registers[0x0C];
            case 0x0D: // IFR (Interrupt Flag Register)
                return _interruptFlag;
            case 0x0E: // IER (Interrupt Enable Register)
                return (byte)(_interruptEnable | 0x80); // Bit 7 is always 1 when reading
            case 0x0F: // Port A (without handshake)
                return ReadPortA();
        }
        
        return 0;
    }
    
    protected override void OnWrite(ushort address, byte data)
    {
        byte reg = (byte)(address - StartAddress);
        
        switch (reg)
        {
            case 0x00: // Port B
                WritePortB(data);
                break;
            case 0x01: // Port A
                WritePortA(data);
                break;
            case 0x02: // DDRB
                DdrB = data;
                break;
            case 0x03: // DDRA
                DdrA = data;
                break;
            case 0x04: // Timer 1 Low
                _timer1Latch = (ushort)((_timer1Latch & 0xFF00) | data);
                break;
            case 0x05: // Timer 1 High
                _timer1Latch = (ushort)((_timer1Latch & 0x00FF) | (data << 8));
                _timer1Counter = _timer1Latch;
                _timer1Running = true;
                _interruptFlag &= 0xBF; // Clear timer 1 interrupt flag
                break;
            case 0x06: // Timer 1 Latch Low
                _timer1Latch = (ushort)((_timer1Latch & 0xFF00) | data);
                break;
            case 0x07: // Timer 1 Latch High
                _timer1Latch = (ushort)((_timer1Latch & 0x00FF) | (data << 8));
                break;
            case 0x0B: // ACR
                _registers[0x0B] = data;
                break;
            case 0x0C: // PCR
                _registers[0x0C] = data;
                break;
            case 0x0D: // IFR
                // Writing 1 to a bit clears it
                _interruptFlag &= (byte)~data;
                break;
            case 0x0E: // IER
                if ((data & 0x80) != 0)
                {
                    // Set bits
                    _interruptEnable |= (byte)(data & 0x7F);
                }
                else
                {
                    // Clear bits
                    _interruptEnable &= (byte)~(data & 0x7F);
                }
                break;
        }
    }
    
    private byte ReadPortA()
    {
        // Read actual port value (simplified)
        return (byte)(PortA & DdrA);
    }
    
    private byte ReadPortB()
    {
        // Read actual port value (simplified)
        return (byte)(PortB & DdrB);
    }
    
    private void WritePortA(byte data)
    {
        PortA = data;
        // Could trigger callbacks for output
    }
    
    private void WritePortB(byte data)
    {
        PortB = data;
        // Could trigger callbacks for output
    }
}
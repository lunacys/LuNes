namespace LuNes.Devices;

public class W65C22 : MemoryMappedDevice
{
    public const byte PORTB = 0x00;
    public const byte PORTA = 0x01;
    public const byte DDRB = 0x02;
    public const byte DDRA = 0x03;
    public const byte T1C_L = 0x04;
    public const byte T1C_H = 0x05;
    public const byte T1L_L = 0x06;
    public const byte T1L_H = 0x07;
    public const byte ACR = 0x0B;
    public const byte PCR = 0x0C;
    public const byte IFR = 0x0D;
    public const byte IER = 0x0E;

    private byte _ddra;
    private byte _ddrb;
    private byte _portALatch; // last value written to PA
    private byte _portBLatch; // last value written to PB
    private byte _inputA; // external levels on PA pins (1 = high)
    private byte _inputB; // external levels on PB pins

    private ushort _t1Counter;
    private ushort _t1Latch;
    private bool _t1Running;
    private bool _t1Continuous; // from ACR

    private byte _pcr; // Peripheral Control Register
    private byte _acr; // Auxiliary Control Register
    private byte _ifr; // Interrupt Flag Register (bits 6‑0)
    private byte _ier; // Interrupt Enable Register (bits 6‑0, bit 7 is enable)

    private bool _ca1Prev; // last level of CA1 pin
    private bool _irqLineLow; // current state of IRQB output
    
    private bool _irqPending = false;
    private bool _t1Pb7Output;

    private SimpleBus _bus;
    
    private readonly Lock _lock = new Lock();
    
    public byte DdrA => _ddra;
    public byte DdrB => _ddrb;
    public byte PortALatch => _portALatch;
    public byte PortBLatch => _portBLatch;
    public byte InputA => _inputA;
    public byte InputB => _inputB;
    
    public ushort T1Counter => _t1Counter;
    public ushort T1Latch => _t1Latch;
    public bool T1Running => _t1Running;
    public bool T1Continuous => _t1Continuous;
    
    public byte Pcr  => _pcr;
    public byte Acr => _acr;
    public byte Ifr => _ifr;
    public byte Ier => _ier;
    
    public bool Ca1Prev => _ca1Prev;
    public bool IrqLineLow => _irqLineLow;
    
    public byte PortA => (byte)((_portALatch & _ddra) | (_inputA & ~_ddra));
    public byte PortB => (byte)((_portBLatch & _ddrb) | (_inputB & ~_ddrb));
    
    public bool LcdBusy { get; set; } = false;

    public W65C22(SimpleBus bus, ushort baseAddress)
        : base(baseAddress, (ushort)(baseAddress + 0x0F))
    {
        _bus = bus;
        Reset();
    }

    public override void Reset()
    {
        _ddra = 0;
        _ddrb = 0;
        _portALatch = 0;
        _portBLatch = 0;
        _inputA = 0xFF; // assume pulled high
        _inputB = 0xFF;
        _t1Counter = 0;
        _t1Latch = 0xFFFF;
        _t1Running = false;
        _t1Continuous = false;
        _pcr = 0;
        _acr = 0;
        _ifr = 0;
        _ier = 0;
        _ca1Prev = true; // idle high
        _irqLineLow = false;
    }

    /// <summary>Set the state of a single PA pin (0‑7). true = high, false = low.</summary>
    public void SetPA(int pin, bool high)
    {
        if (pin < 0 || pin > 7) return;
        byte mask = (byte)(1 << pin);
        if (high)
        {
            _inputA |= mask;
        }
        else
        {
            _inputA &= (byte)~mask;
        }
    }

    /// <summary>Set the state of the CA1 input pin. Used to trigger interrupts.</summary>
    public void SetCA1(bool level)
    {
        lock (_lock)
        {
            bool edge = false;
            if (level != _ca1Prev)
            {
                if ((_pcr & 0x01) == 0 && !level)      // falling edge
                    edge = true;
                else if ((_pcr & 0x01) != 0 && level)   // rising edge
                    edge = true;
                _ca1Prev = level;
            }
            if (edge)
            {
                _ifr |= 0x02;           // set CA1 flag
                _irqPending = true;      // ask emulation thread to check IRQ
            }
        }
    }

    /// <summary>Set all PA inputs from a byte.</summary>
    public void SetPortAInputs(byte value)
    {
        _inputA = value;
    }

    public override void Clock()
    {
        // Timer 1 countdown (if running)
        if (_t1Running && _t1Counter > 0)
        {
            _t1Counter--;
            if (_t1Counter == 0)
            {
                _ifr |= 0x40;
                _irqPending = true;
                UpdateIrqLine();
                if (_t1Continuous)
                {
                    _t1Counter = _t1Latch;
                }
                else
                {
                    _t1Running = false;
                }
            }
        }
        
        ProcessInterrupts();
    }


    protected override byte OnRead(ushort address, bool isReadOnly)
    {
        byte reg = (byte)(address - StartAddress);

        switch (reg)
        {
            
                case PORTB:
               {
                   byte raw = (byte)((_portBLatch & _ddrb) | (_inputB & ~_ddrb));
                   // If PB7 is configured as input, override it with busy flag if LCD is busy
                   if ((_ddrb & 0x80) == 0 && LcdBusy)
                       raw |= 0x80;   // force bit 7 high
                   else if ((_ddrb & 0x80) == 0 && !LcdBusy)
                       raw &= 0x7F;   // force bit 7 low
                   return raw;
               }
             
            // case PORTB:
            // {
            //     byte raw = (byte)((_portBLatch & _ddrb) | (_inputB & ~_ddrb));
            //     // Overwrite PB7 with the effective value
            //     raw = (byte)((raw & 0x7F) | GetEffectivePB7());
            //     return raw;
            // }

            case PORTA:
            case 0x0F:
            {
                // Reading Port A clears the CA1 interrupt flag
                _ifr &= 0xFD;          // clear bit 1
                UpdateIrqLine();
                // fall through to return the port value
                return (byte)((_portALatch & _ddra) | (_inputA & ~_ddra));
            }

            case DDRB: return _ddrb;
            case DDRA: return _ddra;

            case T1C_L:
                // Reading T1C‑L clears the T1 interrupt flag
                _ifr &= 0xBF; // clear bit 6
                UpdateIrqLine();
                return (byte)(_t1Counter & 0xFF);

            case T1C_H:
                return (byte)(_t1Counter >> 8);

            case T1L_L:
                return (byte)(_t1Latch & 0xFF);

            case T1L_H:
                return (byte)(_t1Latch >> 8);

            case ACR: return _acr;
            case PCR: return _pcr;

            case IFR:
                return (byte)(_ifr | 0x80); // bit 7 always 1 when reading

            case IER:
                return (byte)(_ier | 0x80);

            default:
                return 0xFF; // unconnected
        }
    }

    protected override void OnWrite(ushort address, byte data)
    {
        byte reg = (byte)(address - StartAddress);

        switch (reg)
        {
            case PORTB:
                _portBLatch = data;
                break;

            case PORTA:
                _portALatch = data;
                break;

            case DDRB:
                _ddrb = data;
                break;

            case DDRA:
                _ddra = data;
                break;

            case T1C_L:
                // Writing T1C‑L loads the low‑byte of the latch, but does NOT start the timer
                _t1Latch = (ushort)((_t1Latch & 0xFF00) | data);
                break;

            case T1C_H:
                // Writing T1C‑H loads the high byte, clears T1 flag, and starts the timer
                _t1Latch = (ushort)((_t1Latch & 0x00FF) | (data << 8));
                _ifr &= 0xBF; // clear T1 flag
                UpdateIrqLine();

                // Load counter from latch and start
                _t1Counter = _t1Latch;
                _t1Running = true;
                // Timer mode already determined by ACR bits (set elsewhere)
                break;

            case T1L_L:
                _t1Latch = (ushort)((_t1Latch & 0xFF00) | data);
                break;

            case T1L_H:
                _t1Latch = (ushort)((_t1Latch & 0x00FF) | (data << 8));
                break;

            case ACR:
                _acr = data;
                _t1Continuous = (data & 0x40) != 0;    // T1 free‑run
                _t1Pb7Output  = (data & 0x80) != 0;    // T1 pulse on PB7
                break;

            case PCR:
                _pcr = data;
                // CA1 control is in bits 0‑1; we only use bit 0 for edge, ignore handshake
                break;

            case IFR:
                // Writing 1 to a bit clears it
                _ifr &= (byte)~data;
                UpdateIrqLine();
                break;

            case IER:
                if ((data & 0x80) != 0)
                {
                    // Set the specified enable bits
                    _ier |= (byte)(data & 0x7F);
                    // Also set the master enable (bit 7) when any write has bit 7 set
                    _ier |= 0x80;
                }
                else
                {
                    // Clear the specified enable bits
                    _ier &= (byte)~(data & 0x7F);
                    // Master enable is cleared when a write with bit 7 = 0 occurs
                    _ier &= 0x7F;
                }
                UpdateIrqLine();
                break;

            default:
                // Ignore writes to other registers
                break;
        }
    }
    
    /// <summary>Called every cycle on the emulation thread.</summary>
    public void ProcessInterrupts()
    {
        if (_irqPending)
        {
            _irqPending = false;
            UpdateIrqLine();                // recalc IRQ line based on flags
            if (_irqLineLow)
                _bus.Cpu.Irq();             // safe – on emulation thread
        }
    }

    private byte GetEffectivePB7()
    {
        if (_t1Pb7Output)
            return (byte)(_t1Running ? 0x00 : 0x80);   // low when running, high when idle
        if (LcdBusy && (_ddrb & 0x80) == 0)            // PB7 configured as input and LCD busy
            return 0x80;
        // otherwise, return the actual pin state
        return (byte)((_portBLatch & _ddrb & 0x80) | (_inputB & ~_ddrb & 0x80));
    }
    
    private void UpdateIrqLine()
    {
        bool shouldBeLow = (_ier & 0x80) != 0 && ((_ifr & _ier) & 0x7F) != 0;
        _irqLineLow = shouldBeLow;
    }
}
using System.Text;

namespace LuNes.Devices;

public class HD44780 : MemoryMappedDevice
{
    private byte[] _ddRam = new byte[80];  // Display Data RAM
    private byte[] _cgRam = new byte[64];  // Character Generator RAM
    
    private int _cursorPosition;
    private bool _displayOn = true;
    private bool _cursorOn = false;
    private bool _blinkOn = false;
    
    private byte _instructionRegister;
    private bool _registerSelect;  // RS: false=instruction, true=data
    private bool _readWrite;       // R/W: false=write, true=read
    private bool _enable;          // Enable signal
    
    private Queue<byte> _dataBuffer = new();
    
    public HD44780(ushort baseAddress) : base(baseAddress, (ushort)(baseAddress + 0x03))
    {
        Reset();
    }
    
    public void Reset()
    {
        _cursorPosition = 0;
        _displayOn = true;
        _cursorOn = false;
        _blinkOn = false;
        Array.Clear(_ddRam, 0, _ddRam.Length);
        Array.Clear(_cgRam, 0, _cgRam.Length);
        _dataBuffer.Clear();
    }
    
    protected override byte OnRead(ushort address, bool isReadOnly)
    {
        byte reg = (byte)(address - StartAddress);
        
        switch (reg)
        {
            case 0x00: // Command/Status register
                return ReadStatus();
            case 0x01: // Data register
                return ReadData();
            default:
                return 0xFF;
        }
    }
    
    protected override void OnWrite(ushort address, byte data)
    {
        byte reg = (byte)(address - StartAddress);
        
        switch (reg)
        {
            case 0x00: // Command register
                WriteCommand(data);
                break;
            case 0x01: // Data register
                WriteData(data);
                break;
            case 0x02: // Control signals (simplified)
                UpdateControlSignals(data);
                break;
        }
    }
    
    private byte ReadStatus()
    {
        // Busy flag (bit 7) is always clear in this simplified emulation
        // Address counter in lower 7 bits
        return (byte)(_cursorPosition & 0x7F);
    }
    
    private byte ReadData()
    {
        if (_cursorPosition < _ddRam.Length)
            return _ddRam[_cursorPosition++];
        return 0;
    }
    
    private void WriteCommand(byte command)
    {
        if ((command & 0x80) != 0) // Set DDRAM address
        {
            _cursorPosition = command & 0x7F;
        }
        else if ((command & 0x40) != 0) // Set CGRAM address
        {
            _cursorPosition = (command & 0x3F) + 0x40;
        }
        else if ((command & 0x20) != 0) // Function set
        {
            // Ignored in this simple emulation
        }
        else if ((command & 0x10) != 0) // Cursor/display shift
        {
            // Ignored
        }
        else if ((command & 0x08) != 0) // Display on/off control
        {
            _displayOn = (command & 0x04) != 0;
            _cursorOn = (command & 0x02) != 0;
            _blinkOn = (command & 0x01) != 0;
        }
        else if ((command & 0x04) != 0) // Entry mode set
        {
            // Ignored
        }
        else if ((command & 0x02) != 0) // Return home
        {
            _cursorPosition = 0;
        }
        else if ((command & 0x01) != 0) // Clear display
        {
            Array.Clear(_ddRam, 0, _ddRam.Length);
            _cursorPosition = 0;
        }
    }
    
    private void WriteData(byte data)
    {
        if (_cursorPosition < _ddRam.Length)
        {
            _ddRam[_cursorPosition] = data;
            _cursorPosition++;
        }
    }
    
    private void UpdateControlSignals(byte data)
    {
        _registerSelect = (data & 0x01) != 0;
        _readWrite = (data & 0x02) != 0;
        _enable = (data & 0x04) != 0;
        
        if (_enable)
        {
            // Process on rising edge of enable
            // Simplified: just process immediately
        }
    }
    
    public string GetDisplayString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 32; i++)
        {
            if (i == 16) sb.AppendLine();
            char c = (char)_ddRam[i];
            sb.Append(c >= 32 && c <= 126 ? c : ' ');
        }
        return sb.ToString();
    }
}
namespace LuNes;

public class Ppu2C02
{
    protected byte[,] _nameTable;
    protected byte[,] _patternTable;
    protected byte[] _paletteTable;

    protected short _scanline;
    protected short _cycle;
    
    public short Scanline => _scanline;
    public short Cycle => _cycle;
    
    protected Cartridge _cartridge = null!;

    public bool IsFrameComplete;

    public Ppu2C02()
    {
        _nameTable = new byte[2, 1024];
        _patternTable = new byte[2, 4096];
        _paletteTable = new byte[32];
    }
    
    public void ConnectCartridge(Cartridge cartridge)
    {
        _cartridge = cartridge;
    }

    public virtual void Clock()
    {
        _cycle++;
        if (_cycle >= 341)
        {
            _cycle = 0;
            _scanline++;
            if (_scanline >= 261)
            {
                _scanline = -1;
                IsFrameComplete = true;
            }
        }
    }
    
    public byte CpuRead(ushort address, bool isReadOnly = false)
    {
        byte data = 0x00;

        switch (address)
        {
            case 0x0000: // Control
                break;
            case 0x0001: // Mask
                break;
            case 0x0002: // Status
                break;
            case 0x0003: // RAM Address
                break;
            case 0x0004: // RAM Data
                break;
            case 0x0005: // Scroll
                break;
            case 0x0006: // PPU Address
                break;
            case 0x0007: // PPU Data
                break;
        }
        
        return data;
    }

    public byte CpuRead(int address, bool isReadOnly = false) => CpuRead((ushort)address, isReadOnly);

    public void CpuWrite(ushort address, byte data)
    {
        switch (address)
        {
            case 0x0000: // Control
                break;
            case 0x0001: // Mask
                break;
            case 0x0002: // Status
                break;
            case 0x0003: // RAM Address
                break;
            case 0x0004: // RAM Data
                break;
            case 0x0005: // Scroll
                break;
            case 0x0006: // PPU Address
                break;
            case 0x0007: // PPU Data
                break;
        }
    }
    
    public void CpuWrite(int address, byte data) => CpuWrite((ushort) address, data);

    public byte PpuRead(ushort address, bool isReadOnly = false)
    {
        byte data = 0x00;
        address &= 0x3FFF;

        if (_cartridge.PpuRead(address, ref data))
        {
            
        }

        return data;
    }

    public void PpuWrite(ushort address, byte data)
    {
        address &= 0x3FFF;

        if (_cartridge.PpuWrite(address, data))
        {
            
        }
    }
}
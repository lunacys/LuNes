namespace LuNes;

public class NesBus : IBus
{
    //public byte[] CpuRam = new byte[2048];
    public byte[] CpuRam = new byte[64 * 1024];
    public Cpu6502 Cpu { get; }
    public Ppu2C02 Ppu { get; }
    public Cartridge Cartridge { get; private set; } = null!;

    private uint _systemClockCounter = 0;

    public NesBus(Ppu2C02 ppu)
    {
        Cpu = new Cpu6502();
        Cpu.ConnectBus(this);

        Ppu = ppu;
    }

    public NesBus() : this(new Ppu2C02())
    { }

    public void InsertCartridge(Cartridge cartridge)
    {
        Cartridge = cartridge;
        Ppu.ConnectCartridge(Cartridge);
    }

    public void Reset()
    {
        Cpu.Reset();
        _systemClockCounter = 0;
    }

    public void Clock()
    {
        Ppu.Clock();

        if (_systemClockCounter % 3 == 0)
        {
            Cpu.Clock();
        }

        /*if (Ppu.Nmi)
        {
            Ppu.Nmi = false;
            Cpu.Nmi();
        }*/

        _systemClockCounter++;
    }
    
    public byte CpuRead(ushort address, bool isReadOnly = false)
    {
        byte data = 0x00;

        if (Cartridge?.CpuRead(address, ref data) == true)
        {
            
        }
        else if (address >= 0x0000 && address <= 0x1FFF)
        {
            return CpuRam[address & 0x07FF];
        }
        else if (address >= 0x2000 && address <= 0x3FFF)
        {
            return Ppu.CpuRead(address & 0x0007, isReadOnly);
        }

        return data;
    }
    
    public byte CpuRead(int address, bool isReadOnly = false) => CpuRead((ushort)address, isReadOnly);

    public void CpuWrite(ushort address, byte data)
    {
        if (Cartridge.CpuWrite(address, data))
        {
            
        }
        else if (address >= 0x0000 && address <= 0x1FFF)
        {
            CpuRam[address & 0x07FF] = data;
        }
        else if (address >= 0x2000 && address <= 0x3FFF)
        {
            Ppu.CpuWrite(address & 0x0007, data);
        }
    }
}
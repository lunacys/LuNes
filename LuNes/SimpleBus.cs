using LuNes.Cpu;

namespace LuNes;

public class SimpleBus : IBus
{
    public Cpu6502 Cpu { get; }
    private readonly List<IBusDevice> _devices = new();
    private readonly byte[] _ram;

    public ushort RamStart { get; } = 0x0000;
    public ushort RamEnd { get; } = 0x7FFF;
    public ushort RomStart { get; } = 0x8000;
    public ushort RomEnd { get; } = 0xFFFF;
    
    public byte[] Ram => _ram;

    public SimpleBus(int ramSize = 0x8000)
    {
        _ram = new byte[ramSize];
        Cpu = new Cpu6502();
        Cpu.ConnectBus(this);
    }

    public void ConnectDevice(IBusDevice device)
    {
        _devices.Add(device);
    }

    public void Reset()
    {
        Cpu.Reset();

        for (int i = 0; i < _ram.Length; i++)
        {
            _ram[i] = 0x00;
        }
        
        foreach (var device in _devices)
            device.Reset();
    }

    public void Clock()
    {
        Cpu.Clock();
        
        foreach (var device in _devices)
            device.Clock();
    }

    public byte CpuRead(ushort address, bool isReadOnly = false)
    {
        foreach (var device in _devices)
        {
            if (device.CpuRead(address, out byte data, isReadOnly))
                return data;
        }

        if (address >= RamStart && address <= RamEnd)
            return _ram[address - RamStart];

        return 0xFF;
    }

    public byte CpuRead(int address, bool isReadOnly = false)
        => CpuRead((ushort)address, isReadOnly);

    public void CpuWrite(ushort address, byte data)
    {
        foreach (var device in _devices)
        {
            if (device.CpuWrite(address, data))
                return;
        }
        
        if (address >= RamStart && address <= RamEnd)
            _ram[address - RamStart] = data;
    }
}
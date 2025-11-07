namespace LuNes;

public class Bus
{
    public Ram Ram { get; }
    public Cpu6502 Cpu { get; }

    public Bus()
    {
        Ram = new Ram();
        Cpu = new Cpu6502();
        Cpu.ConnectBus(this);
    }
    
    public byte Read(ushort address, bool isReadOnly = false)
    {
        if (address >= 0x0000 && address <= 0xffff)
        {
            return Ram.Data[address];
        }

        return 0;
    }
    
    public byte Read(int address, bool isReadOnly = false) => Read((ushort)address, isReadOnly);

    public void Write(ushort address, byte data)
    {
        if (address >= 0x0000 && address <= 0xffff)
        {
            Ram.Data[address] = data;
        }
    }
}
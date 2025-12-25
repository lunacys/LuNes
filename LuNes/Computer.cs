using LuNes.Devices;

namespace LuNes;

public class Computer
{
    public SimpleBus Bus { get; }
    public W65C22 Via { get; }
    public HD44780 Lcd { get; }
    public RomDevice Rom { get; }
    
    public Computer(byte[]? romData)
    {
        Bus = new SimpleBus();
        
        romData ??= new byte[RomDevice.MaxSize];
        
        // Map devices (based on your breadboard design)
        // Typically:
        // RAM: $0000-$7FFF (32KB)
        // VIA: $6000-$600F (W65C22)
        // LCD: $7000-$7003 (HD44780)
        // ROM: $8000-$FFFF (32KB)
        
        Via = new W65C22(0x6000);
        Lcd = new HD44780(0x7000);
        Rom = new RomDevice(0x8000, romData);
        
        Bus.ConnectDevice(Via);
        Bus.ConnectDevice(Lcd);
        Bus.ConnectDevice(Rom);
        
        // Set reset vector
        Bus.CpuWrite(0xFFFC, 0x00);
        Bus.CpuWrite(0xFFFD, 0x80);
        
        Bus.Reset();
    }
    
    public void RunSingleStep()
    {
        Bus.Clock();
    }
    
    public void RunForCycles(int cycles)
    {
        for (int i = 0; i < cycles; i++)
        {
            Bus.Clock();
        }
    }

    public void LoadRom(byte[] romData)
    {
        Rom.Load(romData);
    }
}
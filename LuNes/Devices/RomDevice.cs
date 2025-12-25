namespace LuNes.Devices;

public class RomDevice : MemoryMappedDevice
{
    private readonly byte[] _data;

    public const int MaxSize = 0x10000 - 0x8000;

    public RomDevice(ushort startAddress, byte[] data) 
        : base(startAddress, (ushort)(startAddress + data.Length - 1))
    {
        _data = new byte[MaxSize];
        Load(data);
    }

    protected override byte OnRead(ushort address, bool isReadOnly)
    {
        return _data[address - StartAddress];
    }

    protected override void OnWrite(ushort address, byte data)
    {
        
    }

    public void Load(byte[] data)
    {
        if (data.Length < MaxSize)
            Console.WriteLine($"WARNING: ROM size is less than {MaxSize} bytes. Got {data.Length} bytes.");
        
        Array.Copy(data, 0, _data, 0, data.Length);
    }
}
namespace LuNes;

public abstract class MemoryMappedDevice : IBusDevice
{
    protected ushort StartAddress;
    protected ushort EndAddress;

    protected MemoryMappedDevice(ushort startAddress, ushort endAddress)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
    }

    public bool CpuRead(ushort address, out byte data, bool isReadOnly = false)
    {
        if (address >= StartAddress && address <= EndAddress)
        {
            data = OnRead(address, isReadOnly);
            return true;
        }

        data = 0;
        return false;
    }

    public bool CpuWrite(ushort address, byte data)
    {
        if (address >= StartAddress && address <= EndAddress)
        {
            OnWrite(address, data);
            return true;
        }

        return false;
    }

    public virtual void Clock() { }
    public virtual void Reset() { }
    
    protected abstract byte OnRead(ushort address, bool isReadOnly);
    protected abstract void OnWrite(ushort address, byte data);
}
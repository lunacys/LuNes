namespace LuNes;

public interface IBusDevice
{
    bool CpuRead(ushort address, out byte data, bool isReadOnly = false);
    bool CpuWrite(ushort address, byte data);

    void Clock();
    void Reset();
}
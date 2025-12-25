namespace LuNes;

public interface IBus
{
    byte CpuRead(ushort address, bool isReadOnly = false);
    byte CpuRead(int address, bool isReadOnly = false);
    void CpuWrite(ushort address, byte data);
}
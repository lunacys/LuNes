namespace LuNes.Mappers;

public abstract class Mapper
{
    protected byte ProgramBanks;
    protected byte CharacterBanks;
    
    protected Mapper(byte programBanks, byte characterBanks)
    {
        ProgramBanks = programBanks;
        CharacterBanks = characterBanks;
    }

    public abstract bool CpuMapRead(ushort address, ref uint mappedAddress);
    public abstract bool CpuMapWrite(ushort address, ref uint mappedAddress);
    
    public bool CpuMapRead(int address, ref uint data) => CpuMapRead((ushort) address, ref data);
    public bool CpuMapWrite(int address, ref uint data) => CpuMapWrite((ushort) address, ref data);
    
    public abstract bool PpuMapRead(ushort address, ref uint mappedAddress);
    public abstract bool PpuMapWrite(ushort address, ref uint mappedAddress);
    
    public bool PpuMapRead(int address, ref uint data) => PpuMapRead((ushort) address, ref data);
    public bool PpuMapWrite(int address, ref uint data) => PpuMapWrite((ushort) address, ref data);
}
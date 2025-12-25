namespace LuNes.Mappers;

public class Mapper0000 : Mapper
{
    public Mapper0000(byte programBanks, byte characterBanks) 
        : base(programBanks, characterBanks)
    {
    }

    public override bool CpuMapRead(ushort address, ref uint mappedAddress)
    {
        // if PRGROM is 16KB
        //     CPU Address Bus          PRG ROM
        //     0x8000 -> 0xBFFF: Map    0x0000 -> 0x3FFF
        //     0xC000 -> 0xFFFF: Mirror 0x0000 -> 0x3FFF
        // if PRGROM is 32KB
        //     CPU Address Bus          PRG ROM
        //     0x8000 -> 0xFFFF: Map    0x0000 -> 0x7FFF	
        if (address >= 0x8000 && address <= 0xFFFF)
        {
            mappedAddress = (uint)(address & (ProgramBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        return false;
    }

    public override bool CpuMapWrite(ushort address, ref uint mappedAddress)
    {
        if (address >= 0x8000 && address <= 0xFFFF)
        {
            mappedAddress = (uint)(address & (ProgramBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        return false;
    }

    public override bool PpuMapRead(ushort address, ref uint mappedAddress)
    {
        // There is no mapping required for PPU
        // PPU Address Bus          CHR ROM
        // 0x0000 -> 0x1FFF: Map    0x0000 -> 0x1FFF
        if (address >= 0x0000 && address <= 0x1FFF)
        {
            mappedAddress = address;
            return true;
        }

        return false;
    }

    public override bool PpuMapWrite(ushort address, ref uint mappedAddress)
    {
        if (address >= 0x0000 && address <= 0x1FFF)
        {
            if (CharacterBanks == 0)
            {
                mappedAddress = address;
                return true;
            }
        }

        return false;
    }
}
namespace LuNes;

public delegate byte Operate();
public delegate byte AddressMode();

public enum AddressModeEnum
{
    Imp, Imm, Zp0, Zpx, Zpy, Rel, Abs, Abx, Aby, Ind, Izx, Izy, Unknown
}

public readonly struct Instruction
{
    public readonly string Name;
    public readonly Operate Operation;
    public readonly AddressMode AddressMode;
    public readonly byte Cycles;
    public readonly AddressModeEnum AddressModeEnum;

    public Instruction(string name, Operate operation, AddressMode addressMode, byte cycles, AddressModeEnum addressModeEnum)
    {
        Name = name;
        Operation = operation;
        AddressMode = addressMode;
        Cycles = cycles;
        AddressModeEnum = addressModeEnum;
    }

    public string GetAddressModeName() => GetAddressModeName(AddressModeEnum);
    
    public static string GetAddressModeName(AddressModeEnum mode)
    {
        return mode switch
        {
            AddressModeEnum.Imp => "IMP",
            AddressModeEnum.Imm => "IMM",
            AddressModeEnum.Zp0 => "ZP0",
            AddressModeEnum.Zpx => "ZPX",
            AddressModeEnum.Zpy => "ZPY",
            AddressModeEnum.Rel => "REL",
            AddressModeEnum.Abs => "ABS",
            AddressModeEnum.Abx => "ABX",
            AddressModeEnum.Aby => "ABY",
            AddressModeEnum.Ind => "IND",
            AddressModeEnum.Izx => "IZX",
            AddressModeEnum.Izy => "IZY",
            AddressModeEnum.Unknown => "???",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}
namespace LuNes;

public delegate byte Operate();
public delegate byte AddressMode();

public readonly struct Instruction
{
    public readonly string Name;
    public readonly Operate Operation;
    public readonly AddressMode AddressMode;
    public readonly byte Cycles;

    public Instruction(string name, Operate operation, AddressMode addressMode, byte cycles)
    {
        Name = name;
        Operation = operation;
        AddressMode = addressMode;
        Cycles = cycles;
    }
}
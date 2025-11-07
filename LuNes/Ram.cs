namespace LuNes;

public class Ram
{
    public const int Size = 64 * 1024;
    
    public byte[] Data { get; }

    public Ram()
    {
        Data = new byte[Size];
    }

    public void Clear()
    {
        for (int i = 0; i < Size; i++)
        {
            Data[i] = 0;
        }
    }
}
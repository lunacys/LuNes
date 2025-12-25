using System.Runtime.InteropServices;
using LuNes.Mappers;

namespace LuNes;

public class Cartridge
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Name;
        public byte PrgRomChunks;
        public byte ChrRomChunks;
        public byte Mapper1;
        public byte Mapper2;
        public byte PrgRamSize;
        public byte TvSystem1;
        public byte TvSystem2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] Unused;
    }

    public enum Mirror
    {
        Horizontal,
        Vertical,
        OneScreenLo,
        OneScreenHi
    }
    
    public bool IsImageValid { get; private set; }

    private byte _mapperId = 0;
    private byte _programBanks = 0;
    private byte _characterBanks = 0;

    private byte[] _programMemory = null!;
    private byte[] _characterMemory = null!;

    private Mapper _mapper = null!;

    public Mirror CurrentMirror = Mirror.Horizontal;

    public Cartridge(string fileName)
    {
        IsImageValid = ReadRom(fileName);
    }
    
    public bool CpuRead(ushort address, ref byte data)
    {
        uint mappedAddress = 0;
        if (_mapper.CpuMapRead(address, ref mappedAddress))
        {
            data = _programMemory[(int)mappedAddress];
            return true;
        }

        return false;
    }

    public bool CpuWrite(ushort address, byte data)
    {
        uint mappedAddress = 0;
        if (_mapper.CpuMapWrite(address, ref mappedAddress))
        {
            _programMemory[(int)mappedAddress] = data;
            return true;
        }

        return false;
    }

    public bool PpuRead(ushort address, ref byte data)
    {
        uint mappedAddress = 0;
        if (_mapper.PpuMapRead(address, ref mappedAddress))
        {
            data = _characterMemory[(int)mappedAddress];
            return true;
        }

        return false;
    }

    public bool PpuWrite(ushort address, byte data)
    {
        uint mappedAddress = 0;
        // TODO: Check if needed to use PpuMapRead instead
        if (_mapper.PpuMapWrite(address, ref mappedAddress))
        {
            _characterMemory[(int)mappedAddress] = data;
            return true;
        }

        return false;
    }

    public bool ReadRom(string filename)
    {
        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            var header = new Header
            {
                Name = br.ReadBytes(4),
                PrgRomChunks = br.ReadByte(),
                ChrRomChunks = br.ReadByte(),
                Mapper1 = br.ReadByte(),
                Mapper2 = br.ReadByte(),
                PrgRamSize = br.ReadByte(),
                TvSystem1 = br.ReadByte(),
                TvSystem2 = br.ReadByte(),
                Unused = br.ReadBytes(5)
            };

            if (header.Name[0] != 'N' || header.Name[1] != 'E' || header.Name[2] != 'S' || header.Name[3] != 0x1A)
            {
                Console.WriteLine("Invalid NES file format");
                return false;
            }

            byte mapperId = (byte)((header.Mapper2 >> 4) << 4 | (header.Mapper1 >> 4));
            CurrentMirror = (header.Mapper1 & 0x01) == 1 ? Mirror.Vertical : Mirror.Horizontal;

            int prgRomSize = header.PrgRomChunks * 16384; // 16KB chunks
            int chrRomSize = header.ChrRomChunks * 8192; // 8KB chunks

            // TODO: Implement file type recognition
            byte[] prgRom = br.ReadBytes(prgRomSize);
            
            byte[] chrRom = header.ChrRomChunks > 0
                ? br.ReadBytes(chrRomSize)
                : new byte[8192]; // 8KB for RAM if no CHR ROM

            _programMemory = prgRom;
            _characterMemory = chrRom;

            switch (mapperId)
            {
                case 0: _mapper = new Mapper0000(_programBanks, _characterBanks); break;
                default: throw new NotImplementedException($"Unknown mapper {mapperId}");
            }

            Console.WriteLine($"Loaded: PRG ROM: {prgRomSize} bytes, CHR ROM: {chrRomSize} bytes, Mapper: {mapperId}");

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading ROM: {e.Message}");
            return false;
        }
    }
}
namespace LuNes.Serial;

public enum BinaryImageFormat
{
    /// <summary>
    /// Detect automatically if possible
    /// </summary>
    Unspecified,

    /// <summary>
    /// Raw Binary image
    /// </summary>
    BIN,
    /// <summary>
    /// 2-Byte header for the defining the load address
    /// </summary>
    PRG,
    /// <summary>
    /// 4-byte header for load address, and image length
    /// </summary>
    SBIN,
    

    /// <summary>
    /// Raw Binary image
    /// </summary>
    RAW = BIN,
    /// <summary>
    /// Commodore PRG
    /// </summary>
    CBM_PRG = PRG,
    /// <summary>
    /// Apple DOS 3.3 binary file header
    /// </summary>
    APPLE_BIN = SBIN
}
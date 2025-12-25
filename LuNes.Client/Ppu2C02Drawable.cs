using Raylib_cs;

namespace LuNes.Client;

public class Ppu2C02Drawable : Ppu2C02, IDisposable
{
    private Color[] _screenPalette;
    private Texture2D _screenTexture;
    private Texture2D[] _nameTableTexture;
    private Texture2D[] _patternTableTexture;

    private Color[] _screenTextureData;

    private Random _random = new Random();

    public Texture2D ScreenTexture => _screenTexture;

    public Ppu2C02Drawable() : base()
    {
        _screenPalette = new Color[0x40];
        InitializeScreenPalette();

        _screenTextureData = new Color[256 * 240];
        for (int y = 0; y < 240; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                var i = y * 256 + x;
                _screenTextureData[i] = _screenPalette[0x3F];
            }
        }

        var screenImage = Raylib.GenImageColor(256, 240, _screenPalette[0x3F]);
        _screenTexture = Raylib.LoadTextureFromImage(screenImage);
        Raylib.UnloadImage(screenImage);

        _nameTableTexture = new Texture2D[2];
        var nt1 = Raylib.GenImageColor(256, 240, _screenPalette[0x3F]);
        var nt2 = Raylib.GenImageColor(256, 240, _screenPalette[0x3F]);

        _nameTableTexture[0] = Raylib.LoadTextureFromImage(nt1);
        _nameTableTexture[1] = Raylib.LoadTextureFromImage(nt2);

        Raylib.UnloadImage(nt1);
        Raylib.UnloadImage(nt2);

        var pt1 = Raylib.GenImageColor(128, 128, _screenPalette[0x3F]);
        var pt2 = Raylib.GenImageColor(128, 128, _screenPalette[0x3F]);

        _patternTableTexture = new Texture2D[2];
        _patternTableTexture[0] = Raylib.LoadTextureFromImage(nt1);
        _patternTableTexture[1] = Raylib.LoadTextureFromImage(nt2);

        Raylib.UnloadImage(pt1);
        Raylib.UnloadImage(pt2);
    }

    public override void Clock()
    {
        if (_scanline < 240 && _scanline >= 0 && _cycle < 256 && _cycle > 0)
        {
            var i = _scanline * 256 + (_cycle - 1);
            _screenTextureData[i] = _screenPalette[_random.Next(0, 2) == 0 ? 0x3F : 0x30];
        }

        base.Clock();
    }

    public Texture2D GetNameTable(byte i) => _nameTableTexture[i];

    public Texture2D GetPatternTable(byte i) => _patternTableTexture[i];

    public void UpdateScreenTexture()
    {
        Raylib.UpdateTexture(_screenTexture, _screenTextureData);
    }

    private void InitializeScreenPalette()
    {
        _screenPalette[0x00] = new Color(84, 84, 84);
        _screenPalette[0x01] = new Color(0, 30, 116);
        _screenPalette[0x02] = new Color(8, 16, 144);
        _screenPalette[0x03] = new Color(48, 0, 136);
        _screenPalette[0x04] = new Color(68, 0, 100);
        _screenPalette[0x05] = new Color(92, 0, 48);
        _screenPalette[0x06] = new Color(84, 4, 0);
        _screenPalette[0x07] = new Color(60, 24, 0);
        _screenPalette[0x08] = new Color(32, 42, 0);
        _screenPalette[0x09] = new Color(8, 58, 0);
        _screenPalette[0x0A] = new Color(0, 64, 0);
        _screenPalette[0x0B] = new Color(0, 60, 0);
        _screenPalette[0x0C] = new Color(0, 50, 60);
        _screenPalette[0x0D] = new Color(0, 0, 0);
        _screenPalette[0x0E] = new Color(0, 0, 0);
        _screenPalette[0x0F] = new Color(0, 0, 0);

        _screenPalette[0x10] = new Color(152, 150, 152);
        _screenPalette[0x11] = new Color(8, 76, 196);
        _screenPalette[0x12] = new Color(48, 50, 236);
        _screenPalette[0x13] = new Color(92, 30, 228);
        _screenPalette[0x14] = new Color(136, 20, 176);
        _screenPalette[0x15] = new Color(160, 20, 100);
        _screenPalette[0x16] = new Color(152, 34, 32);
        _screenPalette[0x17] = new Color(120, 60, 0);
        _screenPalette[0x18] = new Color(84, 90, 0);
        _screenPalette[0x19] = new Color(40, 114, 0);
        _screenPalette[0x1A] = new Color(8, 124, 0);
        _screenPalette[0x1B] = new Color(0, 118, 40);
        _screenPalette[0x1C] = new Color(0, 102, 120);
        _screenPalette[0x1D] = new Color(0, 0, 0);
        _screenPalette[0x1E] = new Color(0, 0, 0);
        _screenPalette[0x1F] = new Color(0, 0, 0);

        _screenPalette[0x20] = new Color(236, 238, 236);
        _screenPalette[0x21] = new Color(76, 154, 236);
        _screenPalette[0x22] = new Color(120, 124, 236);
        _screenPalette[0x23] = new Color(176, 98, 236);
        _screenPalette[0x24] = new Color(228, 84, 236);
        _screenPalette[0x25] = new Color(236, 88, 180);
        _screenPalette[0x26] = new Color(236, 106, 100);
        _screenPalette[0x27] = new Color(212, 136, 32);
        _screenPalette[0x28] = new Color(160, 170, 0);
        _screenPalette[0x29] = new Color(116, 196, 0);
        _screenPalette[0x2A] = new Color(76, 208, 32);
        _screenPalette[0x2B] = new Color(56, 204, 108);
        _screenPalette[0x2C] = new Color(56, 180, 204);
        _screenPalette[0x2D] = new Color(60, 60, 60);
        _screenPalette[0x2E] = new Color(0, 0, 0);
        _screenPalette[0x2F] = new Color(0, 0, 0);

        _screenPalette[0x30] = new Color(236, 238, 236);
        _screenPalette[0x31] = new Color(168, 204, 236);
        _screenPalette[0x32] = new Color(188, 188, 236);
        _screenPalette[0x33] = new Color(212, 178, 236);
        _screenPalette[0x34] = new Color(236, 174, 236);
        _screenPalette[0x35] = new Color(236, 174, 212);
        _screenPalette[0x36] = new Color(236, 180, 176);
        _screenPalette[0x37] = new Color(228, 196, 144);
        _screenPalette[0x38] = new Color(204, 210, 120);
        _screenPalette[0x39] = new Color(180, 222, 120);
        _screenPalette[0x3A] = new Color(168, 226, 144);
        _screenPalette[0x3B] = new Color(152, 226, 180);
        _screenPalette[0x3C] = new Color(160, 214, 228);
        _screenPalette[0x3D] = new Color(160, 162, 160);
        _screenPalette[0x3E] = new Color(0, 0, 0);
        _screenPalette[0x3F] = new Color(0, 0, 0);
    }

    public void Dispose()
    {
        Raylib.UnloadTexture(_screenTexture);
        Raylib.UnloadTexture(_nameTableTexture[0]);
        Raylib.UnloadTexture(_nameTableTexture[1]);
        Raylib.UnloadTexture(_patternTableTexture[0]);
        Raylib.UnloadTexture(_patternTableTexture[1]);
    }
}
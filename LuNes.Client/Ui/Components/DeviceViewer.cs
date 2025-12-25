using System.Numerics;
using Raylib_cs;

namespace LuNes.Client.Ui.Components;

public class DeviceViewer : IComponent
{
    private readonly Computer _computer;
    private readonly Font _font;

    private Rectangle _viaBounds;
    private Rectangle _lcdBounds;
    private const int FontSize = 16;

    public bool IsVisible { get; set; } = true;

    public DeviceViewer(Computer computer, Font font)
    {
        _computer = computer;
        _font = font;
        _viaBounds = new Rectangle(16, 16, 400, 200);
        _lcdBounds = new Rectangle(16, 250, 400, 100);
    }

    public void Update(float deltaTime)
    {
    }

    public void Draw()
    {
        // Draw VIA
        Raylib.DrawRectangleRec(_viaBounds, new Color(0, 0, 0, 128));
        Raylib.DrawRectangleLinesEx(_viaBounds, 2, Color.Gray);

        int x = (int)_viaBounds.X + 10;
        int y = (int)_viaBounds.Y + 10;
        int lineHeight = FontSize + 4;

        DrawText(x, y, "VIA (W65C22):", Color.Green);
        y += lineHeight;
        DrawText(x, y, $"Port A: ${Helpers.Hex(_computer.Via.PortA, 2)}");
        y += lineHeight;
        DrawText(x, y, $"Port B: ${Helpers.Hex(_computer.Via.PortB, 2)}");
        y += lineHeight;
        DrawText(x, y, $"DDRA: ${Helpers.Hex(_computer.Via.DdrA, 2)}");
        y += lineHeight;
        DrawText(x, y, $"DDRB: ${Helpers.Hex(_computer.Via.DdrB, 2)}");

        // Draw LCD
        Raylib.DrawRectangleRec(_lcdBounds, new Color(0, 0, 0, 128));
        Raylib.DrawRectangleLinesEx(_lcdBounds, 2, Color.Gray);

        x = (int)_lcdBounds.X + 10;
        y = (int)_lcdBounds.Y + 10;

        DrawText(x, y, "LCD Display:", Color.Green);
        y += lineHeight;

        string lcdText = _computer.Lcd.GetDisplayString();
        string[] lines = lcdText.Split('\n');
        foreach (var line in lines)
        {
            DrawText(x, y, line);
            y += lineHeight;
        }
    }

    private void DrawText(int x, int y, string text, Color color)
    {
        Raylib.DrawTextEx(_font, text, new Vector2(x, y), FontSize, 1, color);
    }

    private void DrawText(int x, int y, string text)
    {
        DrawText(x, y, text, Color.RayWhite);
    }

    public void Resize(int width, int height)
    {
        _viaBounds = new Rectangle(20, 20, _viaBounds.Width, _viaBounds.Height);
        _lcdBounds = new Rectangle(20, _viaBounds.Height + 40, _lcdBounds.Width, _lcdBounds.Height);
    }
}
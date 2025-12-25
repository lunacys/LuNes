using System.Numerics;
using Raylib_cs;

namespace LuNes.Client.Ui.Components;

public class CpuStatus : IComponent
{
    private readonly SimpleBus _bus;
    private readonly Font _font;

    private Rectangle _bounds;
    private const int FontSize = 32;

    public bool IsVisible { get; set; } = true;

    public CpuStatus(SimpleBus bus, Font font)
    {
        _bus = bus;
        _font = font;
        _bounds = new Rectangle(1048, 48, 400, 300);
    }

    public void Update(float deltaTime)
    {
    }

    public void Draw()
    {
        var cpu = _bus.Cpu;

        Raylib.DrawRectangleRec(_bounds, new Color(0, 0, 0, 128));

        int x = (int)_bounds.X;
        int y = (int)_bounds.Y;
        int lineHeight = FontSize + 4;

        // Draw status flags
        DrawText(x, y, "STATUS:");
        DrawText(x + 130, y, "N", (cpu.Status & Cpu6502.Flags.Negative) == 0 ? Color.Red : Color.Green);
        DrawText(x + 160, y, "V", (cpu.Status & Cpu6502.Flags.Overflow) == 0 ? Color.Red : Color.Green);
        DrawText(x + 190, y, "-", (cpu.Status & Cpu6502.Flags.Unused) == 0 ? Color.Red : Color.Green);
        DrawText(x + 220, y, "B", (cpu.Status & Cpu6502.Flags.Break) == 0 ? Color.Red : Color.Green);
        DrawText(x + 250, y, "D", (cpu.Status & Cpu6502.Flags.DecimalMode) == 0 ? Color.Red : Color.Green);
        DrawText(x + 280, y, "I", (cpu.Status & Cpu6502.Flags.DisableInterrupts) == 0 ? Color.Red : Color.Green);
        DrawText(x + 310, y, "Z", (cpu.Status & Cpu6502.Flags.Zero) == 0 ? Color.Red : Color.Green);
        DrawText(x + 340, y, "C", (cpu.Status & Cpu6502.Flags.CarryBit) == 0 ? Color.Red : Color.Green);

        // Draw registers
        DrawText(x, y + lineHeight, $"PC: ${Helpers.Hex(cpu.Pc, 4)}");
        DrawText(x, y + lineHeight * 2, $"A:  ${Helpers.Hex(cpu.A, 2)}   [{cpu.A}]");
        DrawText(x, y + lineHeight * 3, $"X:  ${Helpers.Hex(cpu.X, 2)}   [{cpu.X}]");
        DrawText(x, y + lineHeight * 4, $"Y:  ${Helpers.Hex(cpu.Y, 2)}   [{cpu.Y}]");
        DrawText(x, y + lineHeight * 5, $"SP: ${Helpers.Hex(cpu.Stkp, 4)}");
        DrawText(x, y + lineHeight * 6, $"Cycles: {cpu.Cycles}");
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
        _bounds = new Rectangle(width - _bounds.Width - 20, 20, _bounds.Width, _bounds.Height);
    }
}
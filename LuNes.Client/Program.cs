using System.Numerics;
using Raylib_cs;

namespace LuNes.Client;

static class Program
{
    private static Bus _bus = new Bus();
    private static Font _font;
    private static int _fontSize = 32;
    private static Dictionary<ushort, string> _mapAsm;
    
    [STAThread]
    public static void Main(string[] args)
    {
        Raylib.InitWindow(2000, 1440, "NES Emulator. Space - Step Instruction. R - Reset. Q - IRQ. W - NMI");
        //Raylib.SetWindowState(ConfigFlags.VSyncHint);
        Raylib.SetTargetFPS(160);
        var s = Raylib.GetWindowScaleDPI();
        //rlImGui.Setup();
        
        _font = Raylib.LoadFont(Path.Combine("Content", "Fonts", "FiraCode-Regular.ttf"));

        var program = "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA EA EA";
        var programBytes = program.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
        ushort offset = 0x8000;

        for (var i = 0; i < programBytes.Length; i++)
        {
            _bus.Ram.Data[offset++] = programBytes[i];
        }

        _bus.Ram.Data[0xFFFC] = 0x00;
        _bus.Ram.Data[0xFFFD] = 0x80;

        _mapAsm = _bus.Cpu.Disassemble(0x0000, 0xFFFF);
        
        _bus.Cpu.Reset();
        //_bus.Cpu.Pc = 0x0400;

        bool run = false;

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(32, 32, 32));

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                do
                {
                    _bus.Cpu.Clock();
                } while (!_bus.Cpu.IsComplete());
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.R)) 
                _bus.Cpu.Reset();
            
            if (Raylib.IsKeyPressed(KeyboardKey.Q))
                _bus.Cpu.Irq();
            
            if (Raylib.IsKeyPressed(KeyboardKey.W))
                _bus.Cpu.Nmi();

            if (Raylib.IsKeyPressed(KeyboardKey.A))
                run = !run;

            if (run)
            {
                do
                {
                    _bus.Cpu.Clock();
                } while (!_bus.Cpu.IsComplete());
            }
            
            DrawRam(16, 16, 0x0000, 16, 16);
            DrawRam(16, 700, 0x8000, 16, 16);
            DrawCpu(1024, 16);
            DrawCode(1024, 302, 26);
            
            // rlImGui.Begin();
            // ImGui.Begin("Hello");
            // ImGui.Text("World");
            // ImGui.End();
            // rlImGui.End();
            
            Raylib.EndDrawing();
        }
        
        //rlImGui.Shutdown();
        Raylib.CloseWindow();
    }

    private static void DrawRam(int x, int y, ushort address, int rows, int columns)
    {
        var ramX = x;
        var ramY = y;

        for (int row = 0; row < rows; row++)
        {
            var offset = $"${Helpers.Hex(address, 4)}:";
            for (int col = 0; col < columns; col++)
            {
                offset += $" {Helpers.Hex(_bus.Read(address, true), 2)}";
                address += 1;
            }
            
            DrawText(ramX, ramY, offset);
            
            ramY += _fontSize + 4;
        }
    }

    private static void DrawCpu(int x, int y)
    {
        DrawText(x, y, "STATUS:");
        DrawText(x + 130, y, "N", (_bus.Cpu.Status & Cpu6502.Flags.Negative) == 0 ? Color.Red : Color.Green);
        DrawText(x + 160, y, "V", (_bus.Cpu.Status & Cpu6502.Flags.Overflow) == 0 ? Color.Red : Color.Green);
        DrawText(x + 190, y, "-", (_bus.Cpu.Status & Cpu6502.Flags.Unused) == 0 ? Color.Red : Color.Green);
        DrawText(x + 220, y, "B", (_bus.Cpu.Status & Cpu6502.Flags.Break) == 0 ? Color.Red : Color.Green);
        DrawText(x + 250, y, "D", (_bus.Cpu.Status & Cpu6502.Flags.DecimalMode) == 0 ? Color.Red : Color.Green);
        DrawText(x + 280, y, "I", (_bus.Cpu.Status & Cpu6502.Flags.DisableInterrupts) == 0 ? Color.Red : Color.Green);
        DrawText(x + 310, y, "Z", (_bus.Cpu.Status & Cpu6502.Flags.Zero) == 0 ? Color.Red : Color.Green);
        DrawText(x + 340, y, "C", (_bus.Cpu.Status & Cpu6502.Flags.CarryBit) == 0 ? Color.Red : Color.Green);
        DrawText(x, y + 34, "PC: $" + Helpers.Hex(_bus.Cpu.Pc, 4));
        DrawText(x, y + 68, "A:  $" + Helpers.Hex(_bus.Cpu.A, 2) + $"   [{_bus.Cpu.A}]");
        DrawText(x, y + 102, "X:  $" + Helpers.Hex(_bus.Cpu.X, 2) + $"   [{_bus.Cpu.X}]");
        DrawText(x, y + 136, "Y:  $" + Helpers.Hex(_bus.Cpu.Y, 2) + $"   [{_bus.Cpu.Y}]");
        DrawText(x, y + 170, "Stack P: $" + Helpers.Hex(_bus.Cpu.Stkp, 4));
    }

    private static void DrawCode(int x, int y, int lines)
    {
        var pc = _bus.Cpu.Pc;
    
        // Get all instruction addresses and sort them
        var instructionAddrs = _mapAsm.Keys.OrderBy(k => k).ToList();
    
        // Find the index of the current PC in the sorted list
        var currentIndex = instructionAddrs.IndexOf(pc);
    
        if (currentIndex == -1)
        {
            // PC not found in disassembly, show default message
            DrawText(x, y, "PC not in disassembly range", Color.Yellow);
            return;
        }
    
        // Calculate start and end indices for display
        var startIndex = Math.Max(0, currentIndex - lines / 2);
        var endIndex = Math.Min(instructionAddrs.Count - 1, currentIndex + lines / 2);
    
        var currentY = y;
    
        // Display instructions
        for (int i = startIndex; i <= endIndex; i++)
        {
            var addr = instructionAddrs[i];
            var instruction = _mapAsm[addr];
        
            // Highlight the currently executing line
            var color = (addr == pc) ? Color.Green : Color.RayWhite;
            DrawText(x, currentY, instruction, color);
        
            currentY += _fontSize + 4;
        }
    }

    private static void DrawText(int x, int y, string text)
    {
        DrawText(x, y, text, Color.RayWhite);
    }

    private static void DrawText(int x, int y, string text, Color color)
    {
        Raylib.DrawTextEx(_font, text, new Vector2(x, y), _fontSize, 1, color);
    }
}
using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace LuNes.Client;

static class Program
{
    private static Cartridge _cartridge;
    private static Bus _nes;
    private static Ppu2C02Drawable _ppu;
    private static Font _font;
    private static int _fontSize = 32;
    private static Dictionary<ushort, string> _mapAsm;
    
    [STAThread]
    public static void Main(string[] args)
    {
        Raylib.InitWindow(2000, 1440, "NES Emulator. Space - Step Instruction. R - Reset. Q - IRQ. W - NMI");
        Raylib.SetTargetFPS(160);
        
        _font = Raylib.LoadFont(Path.Combine("Content", "Fonts", "FiraCode-Regular.ttf"));

        // var program = "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA EA EA";
        // var programBytes = program.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
        // ushort offset = 0x8000;
        //
        // for (var i = 0; i < programBytes.Length; i++)
        // {
        //     _bus.CpuRam[offset++] = programBytes[i];
        // }
        //
        // _bus.CpuRam[0xFFFC] = 0x00;
        // _bus.CpuRam[0xFFFD] = 0x80;
        //
        // _mapAsm = _bus.Cpu.Disassemble(0x0000, 0xFFFF);
        //
        // _bus.Cpu.Reset();
        //_bus.Cpu.Pc = 0x0400;

        _ppu = new Ppu2C02Drawable();
        _nes = new Bus(_ppu);

        _cartridge = new Cartridge(Path.Combine("Content", "ROMs", "nestest.nes"));
        if (!_cartridge.IsImageValid)
            throw new Exception("Cartridge is not valid.");
        
        _nes.InsertCartridge(_cartridge);

        _mapAsm = _nes.Cpu.Disassemble(0x0000, 0xFFFF);
        
        _nes.Reset();

        bool run = false;
        
        Stopwatch sw = new Stopwatch();

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                sw.Restart();
                do { _nes.Clock(); } while (!_nes.Ppu.IsFrameComplete);
                do { _nes.Clock(); } while (_nes.Cpu.IsComplete());
                _ppu.UpdateScreenTexture();
                _nes.Ppu.IsFrameComplete = false;
                sw.Stop();
                Console.WriteLine($"NES Frame Time: {sw.ElapsedMilliseconds}ms");
            }

            if (Raylib.IsKeyPressed(KeyboardKey.C))
            {
                // Step-by-step
                do { _nes.Clock();  } while (!_nes.Cpu.IsComplete());
                do { _nes.Clock(); } while (!_nes.Cpu.IsComplete());
                _ppu.UpdateScreenTexture();
                _nes.Ppu.IsFrameComplete = false;
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.R)) 
                _nes.Cpu.Reset();
            
            if (Raylib.IsKeyPressed(KeyboardKey.Q))
                _nes.Cpu.Irq();
            
            if (Raylib.IsKeyPressed(KeyboardKey.W))
                _nes.Cpu.Nmi();

            if (Raylib.IsKeyPressed(KeyboardKey.A))
                run = !run;

            if (run)
            {
                do
                {
                    _nes.Clock();
                } while (!_nes.Ppu.IsFrameComplete);
                _ppu.UpdateScreenTexture();
                _nes.Ppu.IsFrameComplete = false;
            }
            
            
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(32, 32, 32));
            
            //DrawRam(16, 16, 0x0000, 16, 16);
            //DrawRam(16, 700, 0x8000, 16, 16);
            DrawScreen(16, 16);
            DrawCpu(1048, 16);
            DrawCode(1048, 302, 26);
            
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
                offset += $" {Helpers.Hex(_nes.CpuRead(address, true), 2)}";
                address += 1;
            }
            
            DrawText(ramX, ramY, offset);
            
            ramY += _fontSize + 4;
        }
    }

    private static void DrawCpu(int x, int y)
    {
        DrawText(x, y, "STATUS:");
        DrawText(x + 130, y, "N", (_nes.Cpu.Status & Cpu6502.Flags.Negative) == 0 ? Color.Red : Color.Green);
        DrawText(x + 160, y, "V", (_nes.Cpu.Status & Cpu6502.Flags.Overflow) == 0 ? Color.Red : Color.Green);
        DrawText(x + 190, y, "-", (_nes.Cpu.Status & Cpu6502.Flags.Unused) == 0 ? Color.Red : Color.Green);
        DrawText(x + 220, y, "B", (_nes.Cpu.Status & Cpu6502.Flags.Break) == 0 ? Color.Red : Color.Green);
        DrawText(x + 250, y, "D", (_nes.Cpu.Status & Cpu6502.Flags.DecimalMode) == 0 ? Color.Red : Color.Green);
        DrawText(x + 280, y, "I", (_nes.Cpu.Status & Cpu6502.Flags.DisableInterrupts) == 0 ? Color.Red : Color.Green);
        DrawText(x + 310, y, "Z", (_nes.Cpu.Status & Cpu6502.Flags.Zero) == 0 ? Color.Red : Color.Green);
        DrawText(x + 340, y, "C", (_nes.Cpu.Status & Cpu6502.Flags.CarryBit) == 0 ? Color.Red : Color.Green);
        DrawText(x, y + 34, "PC: $" + Helpers.Hex(_nes.Cpu.Pc, 4));
        DrawText(x, y + 68, "A:  $" + Helpers.Hex(_nes.Cpu.A, 2) + $"   [{_nes.Cpu.A}]");
        DrawText(x, y + 102, "X:  $" + Helpers.Hex(_nes.Cpu.X, 2) + $"   [{_nes.Cpu.X}]");
        DrawText(x, y + 136, "Y:  $" + Helpers.Hex(_nes.Cpu.Y, 2) + $"   [{_nes.Cpu.Y}]");
        DrawText(x, y + 170, "Stack P: $" + Helpers.Hex(_nes.Cpu.Stkp, 4));
        DrawText(x, y + 204, $"PPU Cycle: {_nes.Ppu.Cycle}");
        DrawText(x, y + 238, $"PPU Scanline: {_nes.Ppu.Scanline}");
    }

    private static void DrawCode(int x, int y, int lines)
    {
        var pc = _nes.Cpu.Pc;
    
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

    private static void DrawScreen(int x, int y)
    {
        Raylib.DrawTextureEx(_ppu.ScreenTexture, new Vector2(x, y), 0, 4f, Color.White);
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
using System.Numerics;
using ImGuiNET;
using LuNes.Client.Ui;
using Raylib_cs;
using rlImGui_cs;

namespace LuNes.Client;

static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Raylib.InitWindow(3600, 2000, "NES Emulator. Space - Step Instruction. R - Reset. Q - IRQ. W - NMI");
        Raylib.SetTargetFPS(60);
        
        rlImGui.Setup();
        
        //UiScaler.SetScale(1.25f);
        //UiScaler.ApplyScale();

        // var io = ImGui.GetIO();
        // io.Fonts.AddFontFromFileTTF("Content/Fonts/Silkscreen-Regular.ttf", 16);
        // io.Fonts.Build();
        
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigWindowsMoveFromTitleBarOnly = true;

        io.FontGlobalScale = 2.0f;
        
        var style = ImGui.GetStyle();
        style.ScaleAllSizes(1.25f);

        Startup? startup = null;

        try
        {
            startup = new Startup();
            
            while (!Raylib.WindowShouldClose())
            {
                startup.Update();
            
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(32, 32, 32));
            
                rlImGui.Begin();
            
                startup.Draw();
                ImGui.ShowDemoWindow();
                rlImGui.End();
            
                Raylib.EndDrawing();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"FATAL ERROR: {e.Message}");
        }
        finally
        {
            startup?.Dispose();
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}
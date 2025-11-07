using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace LuNes.Client;

static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Raylib.InitWindow(800, 480, "Hello World!");
        rlImGui.Setup();

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);
            
            Raylib.DrawText("Hello, world!", 12, 12, 20, Color.Black);
            
            rlImGui.Begin();
            ImGui.Begin("Hello");
            ImGui.Text("World");
            ImGui.End();
            rlImGui.End();
            
            Raylib.EndDrawing();
        }
        
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}
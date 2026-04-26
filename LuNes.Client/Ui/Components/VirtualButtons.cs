using System.Numerics;
using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public class VirtualButtons : IComponent
{
    public bool IsVisible { get; set; } = true;
    private Computer _computer;
    private BackgroundEmulator _emulator;
    private bool[] _switches = new bool[5];   // toggle states
    
    private readonly Lock _lock = new Lock();

    public VirtualButtons(Computer computer, BackgroundEmulator emulator)
    {
        _computer = computer; 
        _emulator = emulator;
    }

    public void Update(float deltaTime) { }

    public void Draw()
    {
        ImGui.Begin("Virtual Buttons");

        // Row 1: momentary buttons (held while you click)
        ImGui.Text("Momentary:");
        for (int i = 0; i < 5; i++)
        {
            ImGui.PushID($"btn{i}");
            ImGui.Button($"PA{i}", new Vector2(100, 100));
            bool held = ImGui.IsItemActive();
            ImGui.PopID();
            lock (_lock)
                _computer.Via.SetPA(i, !held); // low when held
            if (i < 4) ImGui.SameLine();
        }

        // Row 2: toggle switches
        // ImGui.Text("Switches:");
        // for (int i = 0; i < 5; i++)
        // {
        //     if (ImGui.Checkbox($"PA{i}##sw{i}", ref _switches[i]))
        //     {
        //         // Toggle changed – nothing special needed
        //     }
        //     _computer.Via.SetPA(i, !_switches[i]);   // low when switch ON
        //     if (i < 4) ImGui.SameLine();
        // }

        // ---- CA1 signal generation (simulates your NAND gate) ----
        byte portA = _computer.Via.PortA;
        byte masked = (byte)(portA & 0x1F);
        bool anyPressed = (masked != 0x1F);       // at least one bit low
        lock (_lock)
            _computer.Via.SetCA1(!anyPressed); // idle HIGH, pressed LOW
        

        ImGui.Text(anyPressed ? "PRESSED" : "NOT PRESSED");
        ImGui.Text($"PA inputs: 0x{masked:X2} (bin {masked:B8})");

        ImGui.End();
    }

    public void Resize(int width, int height) { }
}
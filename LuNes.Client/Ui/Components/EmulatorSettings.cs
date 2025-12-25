using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public class EmulatorSettings : IComponent
{
    public bool IsVisible { get; set; } = true;
    private double _clockSpeedHz;
    private double _clockSpeedHzInput;

    private readonly BackgroundEmulator _emulator;

    public EmulatorSettings(BackgroundEmulator emulator)
    {
        _emulator = emulator;
        _clockSpeedHz = _emulator.TargetClockSpeed;
        _clockSpeedHzInput = _clockSpeedHz;
    }

    public void Update(float deltaTime)
    {
        if (Math.Abs(_clockSpeedHzInput - _clockSpeedHz) > 0.01)
        {
            _clockSpeedHz = _clockSpeedHzInput;
            _emulator.TargetClockSpeed = _clockSpeedHz;
        }
        else
        {
            _clockSpeedHz = _emulator.TargetClockSpeed;
            _clockSpeedHzInput = _clockSpeedHz;
        }
    }

    public void Draw()
    {
        ImGui.Begin("Emulator Settings");

        ImGui.Text("Clock Speed:");

        if (ImGui.Button("1 Hz"))
            _clockSpeedHzInput = 1;
        ImGui.SameLine();
        if (ImGui.Button("1 kHz"))
            _clockSpeedHzInput = 1000;
        ImGui.SameLine();
        if (ImGui.Button("100 kHz"))
            _clockSpeedHzInput = 100000;
        ImGui.SameLine();
        if (ImGui.Button("1 MHz"))
            _clockSpeedHzInput = 1000000;
        ImGui.SameLine();
        if (ImGui.Button("2 MHz"))
            _clockSpeedHzInput = 2000000;

        ImGui.PushItemWidth(150);
        if (ImGui.InputDouble("Hz", ref _clockSpeedHzInput, 1, 100, "%.0f"))
        {
            _clockSpeedHzInput = Math.Clamp(_clockSpeedHzInput, 0.1, 10000000.0);
        }

        ImGui.PopItemWidth();

        ImGui.Separator();
        ImGui.Text("Statistics:");

        ImGui.Text($"Target Speed: {_emulator.TargetClockSpeed:N0} Hz");
        ImGui.Text($"Actual Speed: {_emulator.ActualClockSpeed:N0} Hz");
        ImGui.Text($"Ms per Cycle: {_emulator.MsPerCycle:F6} ms");

        if (_emulator.ActualClockSpeed > 0)
        {
            double ratio = _emulator.ActualClockSpeed / _emulator.TargetClockSpeed * 100.0;
            ImGui.Text($"Achieved: {ratio:F1}% of target");

            if (ratio < 95.0)
                ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0, 1), "Warning: Running slower than target!");
        }

        ImGui.Separator();
        ImGui.Text($"FPS: {_emulator.CurrentFps:F1}");

        ImGui.Separator();
        if (_emulator.RunEmulation)
            ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), "RUNNING");
        else
            ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), "PAUSED");

        ImGui.End();
    }

    public void Resize(int width, int height)
    {
    }
}
using System.Numerics;
using ImGuiNET;
using LuNes.Cpu;

namespace LuNes.Client.Ui.Components;

public class CpuControls : IComponent
{
    public bool IsVisible { get; set; } = true;

    private SimpleBus _bus;

    private bool _c, _z, _i, _d, _b, _u, _v, _n;

    private int _addressToWrite;

    public CpuControls(SimpleBus bus)
    {
        _bus = bus;
    }

    public void Update(float deltaTime)
    {
        _c = (int)(_bus.Cpu.Status & Cpu6502.Flags.CarryBit) != 0;
        _z = (int)(_bus.Cpu.Status & Cpu6502.Flags.Zero) != 0;
        _i = (int)(_bus.Cpu.Status & Cpu6502.Flags.DisableInterrupts) != 0;
        _d = (int)(_bus.Cpu.Status & Cpu6502.Flags.DecimalMode) != 0;
        _b = (int)(_bus.Cpu.Status & Cpu6502.Flags.Break) != 0;
        _u = (int)(_bus.Cpu.Status & Cpu6502.Flags.Unused) != 0;
        _v = (int)(_bus.Cpu.Status & Cpu6502.Flags.Overflow) != 0;
        _n = (int)(_bus.Cpu.Status & Cpu6502.Flags.Negative) != 0;
    }

    public void Draw()
    {
        ImGui.Begin("CPU Controls");
        
        if (ImGui.Button("Clock", new Vector2(140, 32)))
        {
            _bus.Clock();
        }
        Shortcut("C");
        HelpMarker("Proceed single clock cycle");
        ImGui.SameLine();

        if (ImGui.Button("Step", new Vector2(140, 32)))
        {
            do
            {
                _bus.Clock();
            } while (!_bus.Cpu.IsComplete());
        }
        Shortcut("Space");
        HelpMarker("Proceed one whole instruction");

        if (ImGui.Button("Reset Bus", new Vector2(140, 32)))
        {
            _bus.Reset();
        }
        Shortcut("R");
        HelpMarker(
            "Resets all devices connected to the bus, including CPU and RAM. " +
            "ROM is not reset as it is read-only."
        );
        ImGui.SameLine();

        if (ImGui.Button("Reset CPU", new Vector2(140, 32)))
        {
            _bus.Cpu.Reset();
        }
        Shortcut("Ctrl+R");
        HelpMarker("Reset all CPU registers. Takes 8 clock cycles. AddressAbsolute=$FFFC. Stkp=$0xFD.");

        if (ImGui.Button("IRQ", new Vector2(140, 32)))
        {
            _bus.Cpu.Irq();
        }
        Shortcut("Q");
        HelpMarker(
            "Interrupt Request. If interrupts are enabled (I flag is not set), " +
            "push PC then Status Register to the stack and read new PC location from fixed address"
        );
        ImGui.SameLine();

        if (ImGui.Button("NMI", new Vector2(140, 32)))
        {
            _bus.Cpu.Nmi();
        }
        Shortcut("W");
        HelpMarker("Non-maskable Interrupt Request. Same as regular Interrupt Request, but cannot be disabled.");

        
        ImGui.BeginDisabled();
        CpuFlag(ref _c, "C", "Carry Bit"); ImGui.SameLine();
        CpuFlag(ref _i, "I", "Disable Interrupts"); ImGui.SameLine();
        CpuFlag(ref _d, "D", "Decimal Mode"); ImGui.SameLine();
        CpuFlag(ref _b, "B", "Break"); ImGui.SameLine();
        CpuFlag(ref _u, "U", "Unused"); ImGui.SameLine();
        CpuFlag(ref _v, "V", "Overflow"); ImGui.SameLine();
        CpuFlag(ref _n, "N", "Negative"); 
        ImGui.EndDisabled();
        
        ImGui.End();
    }

    public void Resize(int width, int height) { }

    private void CpuFlag(ref bool checkbox, string name, string desc)
    {
        if (checkbox)
        {
            ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(0, 1, 0, 1));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 1, 0, 1));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(1, 0, 0, 1));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
        }
        
        ImGui.Checkbox(name, ref checkbox); 
        HelpMarker(desc);
        
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
    }
    
    private void HelpMarker(string desc)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    private void Shortcut(string shortcut)
    {
        ImGui.SameLine();
        ImGui.TextDisabled($"({shortcut})");
    }
}
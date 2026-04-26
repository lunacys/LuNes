using System.Numerics;
using ImGuiNET;
using LuNes.Cpu;

namespace LuNes.Client.Ui.Components;

public class CpuControls : IComponent
{
    public bool IsVisible { get; set; } = true;

    private SimpleBus _bus;
    private BackgroundEmulator _emulator;

    private bool _c, _z, _i, _d, _b, _u, _v, _n;
    private bool _isRunning;

    private int _addressToWrite;

    public CpuControls(SimpleBus bus, BackgroundEmulator emulator)
    {
        _bus = bus;
        _emulator = emulator;
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
        ImGui.SameLine();
        
        if (ImGui.Checkbox("Is Running", ref _emulator.RunEmulation))
        { }

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

        ImGui.Separator();
        
        ImGui.BeginDisabled();
        CpuFlag(ref _c, "C", "Carry Bit"); ImGui.SameLine();
        CpuFlag(ref _i, "I", "Disable Interrupts"); ImGui.SameLine();
        CpuFlag(ref _d, "D", "Decimal Mode"); ImGui.SameLine();
        CpuFlag(ref _b, "B", "Break"); ImGui.SameLine();
        CpuFlag(ref _u, "U", "Unused"); ImGui.SameLine();
        CpuFlag(ref _v, "V", "Overflow"); ImGui.SameLine();
        CpuFlag(ref _n, "N", "Negative"); 
        ImGui.EndDisabled();
        
        ImGui.Separator();

        if (ImGui.BeginTable("Registers", 2,
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Register", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
            
            ImGui.TableHeadersRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("PC");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.Pc:X4} ({_bus.Cpu.Pc})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Prev PC");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.PrevPc:X4} ({_bus.Cpu.PrevPc})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("A");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.A:X2} ({_bus.Cpu.A})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("X");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.X:X2} ({_bus.Cpu.X})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Y");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.Y:X2} ({_bus.Cpu.Y})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("SP");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.Stkp:X4} ({_bus.Cpu.Stkp})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Cycles");
            ImGui.TableNextColumn();
            ImGui.Text($"{_bus.Cpu.Cycles} cycles");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Absolute Addr");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.AddressAbsolute:X4} ({_bus.Cpu.AddressAbsolute})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Relative Addr");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.AddressRelative:X4} ({_bus.Cpu.AddressRelative})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Clock Count");
            ImGui.TableNextColumn();
            ImGui.Text($"{_bus.Cpu.ClockCount}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Fetched");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.Fetched:X2} ({_bus.Cpu.AddressAbsolute})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Opcode");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{_bus.Cpu.Opcode:X2} ({_bus.Cpu.Lookup[_bus.Cpu.Opcode].Name})");
            
            ImGui.EndTable();
        }

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
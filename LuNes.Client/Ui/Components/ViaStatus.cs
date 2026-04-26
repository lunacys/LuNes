using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public class ViaStatus : IComponent
{
    public bool IsVisible { get; set; } = true;

    private Computer _computer;
    
    public ViaStatus(Computer computer)
    {
        _computer = computer;
    }
    
    public void Update(float deltaTime)
    { }

    public void Draw()
    {
        ImGui.Begin("VIA Status");

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
            ImGui.Text("DDR A");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.DdrA} (0x{_computer.Via.DdrA:X2} | 0b{_computer.Via.DdrA:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("DDR B");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.DdrB} (0x{_computer.Via.DdrB:X2} | 0b{_computer.Via.DdrB:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Port A");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.PortA} (0x{_computer.Via.PortA:X2} | 0b{_computer.Via.PortA:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Port B");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.PortB} (0x{_computer.Via.PortB:X2} | 0b{_computer.Via.PortB:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Port A Latch");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.PortALatch} (0x{_computer.Via.PortALatch:X2} | 0b{_computer.Via.PortALatch:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Port B Latch");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.PortBLatch} (0x{_computer.Via.PortBLatch:X2} | 0b{_computer.Via.PortBLatch:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Input A");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.InputA} (0x{_computer.Via.InputA:X2} | 0b{_computer.Via.InputA:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Input B");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.InputB} (0x{_computer.Via.InputB:X2} | 0b{_computer.Via.InputB:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("PCR");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.Pcr} (0x{_computer.Via.Pcr:X2} | 0b{_computer.Via.Pcr:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("ACR");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.Acr} (0x{_computer.Via.Acr:X2} | 0b{_computer.Via.Acr:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("IFR");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.Ifr} (0x{_computer.Via.Ifr:X2} | 0b{_computer.Via.Ifr:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("IER");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.Ier} (0x{_computer.Via.Ier:X2} | 0b{_computer.Via.Ier:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("T1 Counter");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.T1Counter} (0x{_computer.Via.T1Counter:X4})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("T1 Latch");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.T1Latch} (0x{_computer.Via.T1Latch:X4})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("T1 Running");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.T1Running}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("T1 Continuous");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.T1Continuous}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("CA1 Previous");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.Ca1Prev}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("IRQ Line Low");
            ImGui.TableNextColumn();
            ImGui.Text($"{_computer.Via.IrqLineLow}");
            
            ImGui.EndTable();
        }
        
        ImGui.End();
    }

    public void Resize(int width, int height)
    {
        
    }
}
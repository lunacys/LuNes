using ImGuiNET;
using LuNes.Cpu;

namespace LuNes.Client.Ui.Components;

public class DisassemblyViewerTable : IComponent
{
    private readonly SimpleBus _bus;
    private Dictionary<ushort, Cpu6502.DisassembledInstruction> _disassembly = new();
    private List<(ushort Address, Cpu6502.DisassembledInstruction Instruction)> _sortedInstructions = new();
    
    private ushort _currentPc;
    private int _scrollToIndex = -1;
    private string _searchText = "";
    
    public bool IsVisible { get; set; } = true;

    public DisassemblyViewerTable(SimpleBus bus)
    {
        _bus = bus;
        UpdateDisassembly();
        
        _currentPc = _bus.Cpu.Pc;
        _scrollToIndex = _sortedInstructions.FindIndex(x => x.Address == _currentPc);
    }

    public void UpdateDisassembly()
    {
        _disassembly = _bus.Cpu.DisassembleDetailed(0x0000, 0xFFFF);
        _sortedInstructions = _disassembly
            .OrderBy(kv => kv.Key)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    public void Update(float deltaTime)
    {
        _currentPc = _bus.Cpu.Pc;
    }

    public unsafe void Draw()
    {
        ImGui.Begin("Disassembly");
        
        // Control bar
        if (ImGui.Button("Refresh"))
        {
            UpdateDisassembly();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Jump to PC"))
        {
            _scrollToIndex = _sortedInstructions.FindIndex(x => x.Address == _currentPc);
        }
        
        ImGui.SameLine();
        ImGui.Text($"PC: ${_currentPc:X4}");
        
        ImGui.SameLine();
        ImGui.PushItemWidth(200);
        ImGui.InputText("Search", ref _searchText, 256);
        ImGui.PopItemWidth();
        
        ImGui.Separator();
        
        // Create table
        if (ImGui.BeginTable("DisassemblyTable", 5, 
                ImGuiTableFlags.Borders | 
                ImGuiTableFlags.RowBg | 
                ImGuiTableFlags.ScrollY | 
                ImGuiTableFlags.Resizable))
        {
            // Setup columns
            ImGui.TableSetupColumn("Address", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Bytes", ImGuiTableColumnFlags.WidthFixed, 80);
            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Instruction", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Mode", ImGuiTableColumnFlags.WidthFixed, 40);

            ImGui.TableHeadersRow();

            var sy = ImGui.GetScrollY();
            float windowHeight = ImGui.GetWindowHeight();
            // Use clipper for virtual scrolling
            var clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();
            ImGuiNative.ImGuiListClipper_Begin(clipper, _sortedInstructions.Count, -1);

            while (ImGuiNative.ImGuiListClipper_Step(clipper) != 0)
            {
                for (int i = (*clipper).DisplayStart; i < (*clipper).DisplayEnd; i++)
                {
                    var (address, instruction) = _sortedInstructions[i];
                    bool isCurrent = address == _currentPc;

                    ImGui.TableNextRow();

                    // Highlight current row
                    if (isCurrent)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 1, 0, 1));
                    }

                    // Address column
                    ImGui.TableNextColumn();
                    ImGui.Text($"${address:X4}");

                    // Bytes column
                    ImGui.TableNextColumn();
                    string bytesText = FormatBytes(instruction.Bytes);
                    ImGui.TextColored(new System.Numerics.Vector4(0.7f, 0.7f, 0.7f, 1), bytesText);

                    // Label column
                    ImGui.TableNextColumn();
                    if (instruction.HasLabel)
                    {
                        ImGui.Text(instruction.Label);
                    }
                    else
                    {
                        ImGui.TextDisabled("");
                    }

                    // Instruction column
                    ImGui.TableNextColumn();
                    ImGui.Text(instruction.InstructionText);

                    // Mode column
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled($"{{{instruction.AddressingMode}}}");

                    if (isCurrent)
                    {
                        ImGui.PopStyleColor();
                    }
                }
            }

            ImGuiNative.ImGuiListClipper_destroy(clipper);

            
            // Auto-scroll to PC if requested
            if (_scrollToIndex >= 0)
            {
                float rowHeight = ImGui.GetTextLineHeightWithSpacing();
                float scrollY = _scrollToIndex * rowHeight - windowHeight / 2 - 16300;
                ImGui.SetScrollY(Math.Max(0, scrollY));
                _scrollToIndex = -1;
            }
            
            ImGui.EndTable();
        }
        
        ImGui.End();
    }
    
    private string FormatBytes(byte[] bytes)
    {
        string hex = "";
        foreach (var b in bytes)
        {
            hex += $"{b:X2} ";
        }
        return hex.Trim();
    }

    public void Resize(int width, int height) { }
}
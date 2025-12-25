using System.Numerics;
using ImGuiNET;
using LuNes.Cpu;

namespace LuNes.Client.Ui.Components;

public class DisassemblyViewer : IComponent
{
    private readonly SimpleBus _bus;
    private Dictionary<ushort, Cpu6502.DisassembledInstruction> _disassembly = new();
    private List<(ushort Address, Cpu6502.DisassembledInstruction Instruction)> _sortedInstructions = new();
    
    private ushort _currentPc;
    private int _scrollToIndex = -1;
    private Vector2 _scrollPosition;
    private string _searchText = "";
    
    // Color definitions
    private System.Numerics.Vector4 _colorAddress = new(0.7f, 0.7f, 0.7f, 1.0f);      // Light gray
    private System.Numerics.Vector4 _colorBytes = new(0.5f, 0.5f, 0.5f, 1.0f);       // Darker gray
    private System.Numerics.Vector4 _colorLabel = new(1.0f, 1.0f, 1.0f, 1.0f);       // White
    private System.Numerics.Vector4 _colorInstruction = new(1.0f, 1.0f, 1.0f, 1.0f); // White
    private System.Numerics.Vector4 _colorMode = new(0.7f, 0.7f, 0.7f, 1.0f);        // Light gray
    private System.Numerics.Vector4 _colorCurrent = new(0.0f, 1.0f, 0.0f, 1.0f);     // Green for current PC
    
    public bool IsVisible { get; set; } = true;

    public DisassemblyViewer(SimpleBus bus)
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
        ImGui.Begin("Disassembly", ImGuiWindowFlags.HorizontalScrollbar);
        
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
        if (ImGui.InputText("Search", ref _searchText, 256))
        {
            // Search logic could be added here
        }
        ImGui.PopItemWidth();
        
        ImGui.Separator();
        
        // Virtual scrolling disassembly
        float windowHeight = ImGui.GetWindowHeight() - 100;
        ImGui.BeginChild("##DisassemblyScrolling", new System.Numerics.Vector2(0, windowHeight), ImGuiChildFlags.None);
        
        // Use clipper for virtual scrolling
        var clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();
        ImGuiNative.ImGuiListClipper_Begin(clipper, _sortedInstructions.Count, -1);
        
        while (ImGuiNative.ImGuiListClipper_Step(clipper) != 0)
        {
            for (int i = (*clipper).DisplayStart; i < (*clipper).DisplayEnd; i++)
            {
                var (address, instruction) = _sortedInstructions[i];
                bool isCurrent = address == _currentPc;
                
                // Start a new line
                ImGui.PushID($"asm_{address:X4}");
                
                // Highlight current line
                if (isCurrent)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 1, 0, 1));
                    ImGui.PushStyleColor(ImGuiCol.Header, new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1));
                    ImGui.Selectable($"##{i}", true);
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    
                }
                
                // Address column (light gray)
                ImGui.PushStyleColor(ImGuiCol.Text, _colorAddress);
                ImGui.Text($"${address:X4}");
                ImGui.PopStyleColor();
                
                ImGui.SameLine();
                
                // Bytes column (darker gray)
                string bytesText = FormatBytes(instruction.Bytes);
                ImGui.PushStyleColor(ImGuiCol.Text, _colorBytes);
                ImGui.Text(bytesText);
                ImGui.PopStyleColor();
                
                ImGui.SameLine();
                
                // Label column (white, if exists)
                if (instruction.HasLabel)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, _colorLabel);
                    ImGui.Text(instruction.Label);
                    ImGui.PopStyleColor();
                }
                else
                {
                    // Empty space for alignment
                    ImGui.Text("        ");
                }
                
                ImGui.SameLine();
                
                // Instruction column (white or green for current)
                if (isCurrent)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, _colorCurrent);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, _colorInstruction);
                }
                ImGui.Text(instruction.InstructionText);
                ImGui.PopStyleColor();
                
                ImGui.SameLine();
                
                // Addressing mode (light gray)
                ImGui.PushStyleColor(ImGuiCol.Text, _colorMode);
                ImGui.Text($"{{{instruction.AddressingMode}}}");
                ImGui.PopStyleColor();
                
                ImGui.PopID();
            }
        }
        
        ImGuiNative.ImGuiListClipper_destroy(clipper);
        
        // Auto-scroll to PC if requested
        if (_scrollToIndex >= 0)
        {
            float itemHeight = ImGui.GetTextLineHeightWithSpacing();
            var scrollY = _scrollToIndex * itemHeight - windowHeight / 2;
            ImGui.SetScrollY(scrollY);
            _scrollToIndex = -1;
        }
        
        ImGui.EndChild();
        ImGui.End();
    }
    
    private string FormatBytes(byte[] bytes)
    {
        // Format bytes as hex string with fixed width (8 chars)
        string hex = "";
        foreach (var b in bytes)
        {
            hex += $"{b:X2} ";
        }
        
        // Pad to 8 characters for alignment
        return hex.PadRight(8);
    }

    public void Resize(int width, int height) { }
}
using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public class DisassemblyViewer : IComponent
{
    private readonly SimpleBus _bus;
    private Dictionary<ushort, string> _disassembly = new();
    private List<(ushort Address, string Line)> _sortedLines = new();
    
    private ushort _currentPc;
    private int _scrollToIndex;
    
    public bool IsVisible { get; set; } = true;

    public DisassemblyViewer(SimpleBus bus)
    {
        _bus = bus;
        UpdateDisassembly();
        
        _currentPc = _bus.Cpu.Pc;
        _scrollToIndex = _sortedLines.FindIndex(x => x.Address == _currentPc);
    }

    public void UpdateDisassembly()
    {
        _disassembly = _bus.Cpu.Disassemble(0x0000, 0xFFFF);
        _sortedLines = _disassembly
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
        
        if (ImGui.Button("Refresh"))
        {
            UpdateDisassembly();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Jump to PC"))
        {
            _scrollToIndex = _sortedLines.FindIndex(x => x.Address == _currentPc);
        }
        
        ImGui.SameLine();
        ImGui.Text($"PC: ${_currentPc:X4}");
        
        ImGui.Separator();
        
        float windowHeight = ImGui.GetWindowHeight() - 100;
        ImGui.BeginChild("##DisassemblyScrolling", new System.Numerics.Vector2(0, windowHeight), ImGuiChildFlags.None);
        
        var clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();
        ImGuiNative.ImGuiListClipper_Begin(clipper, _sortedLines.Count, -1);
        
        while (ImGuiNative.ImGuiListClipper_Step(clipper) != 0)
        {
            for (int i = (*clipper).DisplayStart; i < (*clipper).DisplayEnd; i++)
            {
                var (address, line) = _sortedLines[i];
                
                // Highlight current PC
                if (address == _currentPc)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 1, 0, 1));
                    ImGui.PushStyleColor(ImGuiCol.Header, new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1));
                    ImGui.Selectable($"##{i}", true);
                    ImGui.SameLine();
                }
                
                ImGui.Text(line);
                
                if (address == _currentPc)
                {
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                }
            }
        }
        
        if (_scrollToIndex >= 0)
        {
            float itemHeight = ImGui.GetTextLineHeightWithSpacing();
            ImGui.SetScrollY(_scrollToIndex * itemHeight - windowHeight / 2);
            _scrollToIndex = -1;
        }
        
        ImGuiNative.ImGuiListClipper_destroy(clipper);
        
        ImGui.EndChild();
        ImGui.End();
    }

    public void Resize(int width, int height) { }
}
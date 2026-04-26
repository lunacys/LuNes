using System.Numerics;
using System.Text;
using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public class RomViewer : IComponent
{
    public bool IsVisible { get; set; } = true;

    private Computer _computer;

    public RomViewer(Computer computer)
    {
        _computer = computer;
    }

    public void Update(float deltaTime)
    {
        
    }

    public unsafe void Draw()
    {
        ImGui.Begin("ROM Viewer");
        
        var memData = _computer.Rom.Data;
        int memSize = memData.Length;
        int baseDisplayAddr = 0x8000;

        int rows = 16;
        float lineHeight = ImGui.GetTextLineHeight();
        int lineTotalCount = (memSize + rows - 1) / rows;
        
        ImGui.SetNextWindowContentSize(new Vector2(0.0f, lineTotalCount * lineHeight));
        ImGui.BeginChild("##scrolling", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), ImGuiChildFlags.None);
        
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        
        int addrDigitsCount = 0;
        for (int n = baseDisplayAddr + memSize - 1; n > 0; n >>= 4)
            addrDigitsCount++;
        
        float glyphWidth = ImGui.CalcTextSize("F").X;
        float cellWidth = glyphWidth * 3; // include trailing space in the width
        
        var clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();
        ImGuiNative.ImGuiListClipper_Begin(clipper, lineTotalCount, lineHeight);

        while (ImGuiNative.ImGuiListClipper_Step(clipper) != 0)
        {
            for (int line_i = (*clipper).DisplayStart; line_i < (*clipper).DisplayEnd; line_i++)
            {
                int addr = line_i * rows;
                ImGui.Text($"${FixedHex(baseDisplayAddr + addr, addrDigitsCount)}: ");
                ImGui.SameLine();
                
                if (memData[addr] == 0)
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1));
                else
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                
                float lineStartX = ImGui.GetCursorPosX();
                for (int n = 0; n < rows && addr < memSize; n++, addr++)
                {
                    ImGui.SameLine(lineStartX + cellWidth * n);
                    
                    ImGui.Text(FixedHex(memData[addr], 2));
                }
                
                ImGui.PopStyleColor();
                
                ImGui.SameLine(lineStartX + cellWidth * rows + glyphWidth * 2);
                
                // Draw ASCII values
                addr = line_i * rows;
                var asciiVal = new StringBuilder(2 + rows);
                asciiVal.Append("| ");
                for (int n = 0; n < rows && addr < memSize; n++, addr++)
                {
                    int c = memData[addr];
                    asciiVal.Append((c >= 32 && c < 128) ? Convert.ToChar(c) : '.');
                }

                ImGui.TextUnformatted(asciiVal.ToString());
            }
        }
        
        ImGuiNative.ImGuiListClipper_destroy(clipper);
        
        ImGui.PopStyleVar(2);
        ImGui.EndChild();

        ImGui.End();
    }

    public void Resize(int width, int height)
    { }
    
    private static string FixedHex(int v, int count)
    {
        return v.ToString($"X{count}");
    }
}
using System.Numerics;
using ImGuiNET;
using LuNes.Devices;

namespace LuNes.Client.Ui.Components;

public class LcdDisplay : IComponent
{
    public bool IsVisible { get; set; } = true;
    private readonly HD44780 _lcd;
    private readonly LcdInterface _interface;
    private readonly string _title;

    private const int Scale = 8;        // <— double the size
    private const int CharWidth  = 5;
    private const int CharHeight = 8;
    private const int ScreenWidth  = 16;
    private const int ScreenHeight = 2;

    private Vector4 _bgColor    = new(0.0f, 0.4f, 0.0f, 1.0f);   // green background
    private Vector4 _pixelOn    = new(1.0f, 1.0f, 1.0f, 1.0f);   // white pixels
    private Vector4 _pixelOff   = new(0.0f, 0.4f, 0.0f, 1.0f);   // matches background

    public LcdDisplay(HD44780 lcd, LcdInterface lcdInterface, string title = "LCD")
    {
        _lcd = lcd;
        _interface = lcdInterface;
        _title = title;
    }

    public void Update(float deltaTime) { }

    public void Draw()
    {
        ImGui.Begin(_title);

        Vector2 origin = ImGui.GetCursorScreenPos();

        for (int row = 0; row < ScreenHeight; row++)
        {
            for (int col = 0; col < ScreenWidth; col++)
            {
                int baseAddr = (row == 0) ? 0x00 : 0x40;
                int shiftedCol = (col + _lcd.DisplayShift) % 40;
                int addr = baseAddr + shiftedCol;
                byte charCode = _lcd.ReadDDRam(addr);
                byte[] pattern = _lcd.GetCharPattern(charCode);

                for (int y = 0; y < 8; y++)
                {
                    byte line = pattern[y];
                    for (int x = 0; x < 5; x++)
                    {
                        bool on = (line & (1 << (4 - x))) != 0;
                        Vector4 colVec = on ? _pixelOn : _pixelOff;
                        Vector2 min = origin + new Vector2(
                            (col * CharWidth + x) * Scale,
                            (row * CharHeight + y) * Scale);
                        Vector2 max = min + new Vector2(Scale, Scale);
                        ImGui.GetWindowDrawList().AddRectFilled(min, max,
                            ImGui.ColorConvertFloat4ToU32(colVec));
                    }
                }
            }
        }

        ImGui.Dummy(new Vector2(ScreenWidth * CharWidth * Scale,
                                ScreenHeight * CharHeight * Scale));
        
        if (ImGui.CollapsingHeader("CGRAM / DDRAM"))
        {
            // DDRAM dump
            ImGui.Text("DDRAM:");
            ImGui.BeginChild("##ddram", new Vector2(0, 3 * ImGui.GetTextLineHeightWithSpacing()));
            // Print 16 chars per line for row0 and row1
            for (int addr = 0; addr < 16; addr++)
            {
                ImGui.SameLine();
                ImGui.Text($"{_lcd.ReadDDRam(addr):X2}");
            }
            ImGui.NewLine();
            for (int addr = 0x40; addr < 0x50; addr++)
            {
                ImGui.SameLine();
                ImGui.Text($"{_lcd.ReadDDRam(addr):X2}");
            }
            ImGui.EndChild();

            // CGRAM dump
            ImGui.Text("CGRAM:");
            ImGui.BeginChild("##cgram", new Vector2(0, 5 * ImGui.GetTextLineHeightWithSpacing()));
            for (int ch = 0; ch < 8; ch++)
            {
                var pat = _lcd.GetCharPattern((byte)ch);
                string line = $"Char {ch}: ";
                foreach (var b in pat) line += $"{b:X2} ";
                ImGui.Text(line);
            }
            ImGui.EndChild();
        }
        
        ImGui.SeparatorText("LCD");
        if (ImGui.BeginTable("LCD Data", 2,
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Register", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
            
            ImGui.TableHeadersRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Last Port A");
            ImGui.TableNextColumn();
            ImGui.Text($"{_interface.LastPortA} (0x{_interface.LastPortA:X2} | 0b{_interface.LastPortA:B8})");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Last Port B");
            ImGui.TableNextColumn();
            ImGui.Text($"{_interface.LastPortA} (0x{_interface.LastPortA:X2} | 0b{_interface.LastPortA:B8})");
            
            ImGui.TableNextColumn();
            ImGui.Text("Cursor Position");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.CursorPos}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Busy Cycles");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.BusyCycles}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Is Busy");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.IsBusy}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Display On");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.DisplayOn}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Cursor On");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.CursorOn}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Blink On");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.BlinkOn}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Entry Mode Inc");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.EntryModeInc}");
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text("Entry Mode Shift");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.EntryModeShift}");
            
            ImGui.TableNextColumn();
            ImGui.Text("Display Shift");
            ImGui.TableNextColumn();
            ImGui.Text($"{_lcd.DisplayShift}");
            
            ImGui.EndTable();
        }
        

        ImGui.End();
    }

    public void Resize(int width, int height) { }
}
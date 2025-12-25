using System.Globalization;
using System.Numerics;
using System.Text;
using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public unsafe class MemoryEditor : IComponent
{
    private readonly SimpleBus _bus;
    private int _rows = 16;
    private int _dataEditingAddr = -1;
    private bool _dataEditingTakeFocus;
    private byte[] _dataInput = new byte[32];
    private byte[] _addrInput = new byte[32];
    private bool _allowEdits = true;
    private string _windowTitle = "Memory Editor";

    // Cache for the window ID to ensure unique ImGui IDs
    private string _windowId;
    private int _instanceId;
    private static int _nextInstanceId = 0;

    public bool IsVisible { get; set; } = true;

    public bool AllowEdits
    {
        get => _allowEdits;
        set => _allowEdits = value;
    }

    public MemoryEditor(SimpleBus bus, string title = "RAM Viewer")
    {
        _bus = bus;
        _windowTitle = title;
        _instanceId = Interlocked.Increment(ref _nextInstanceId);
        _windowId = $"{_windowTitle}##{_instanceId}";
    }

    private static string FixedHex(int v, int count)
    {
        return v.ToString($"X{count}");
    }

    private static bool TryHexParse(byte[] bytes, out int result)
    {
        string input = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
        if (string.IsNullOrEmpty(input))
        {
            result = 0;
            return false;
        }

        return int.TryParse(input, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result);
    }

    private static void ReplaceChars(byte[] bytes, string input)
    {
        var address = Encoding.ASCII.GetBytes(input);
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (i < address.Length) ? address[i] : (byte)0;
        }
    }

    public void Update(float deltaTime)
    {
    }

    public void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 400), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin(_windowId))
        {
            ImGui.End();
            return;
        }

        var memData = _bus.Ram;
        int memSize = memData.Length;
        int baseDisplayAddr = 0x0000;

        float lineHeight = ImGui.GetTextLineHeight();
        int lineTotalCount = (memSize + _rows - 1) / _rows;

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
                int addr = line_i * _rows;
                ImGui.Text($"${FixedHex(baseDisplayAddr + addr, addrDigitsCount)}: ");
                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                // Draw Hexadecimal
                float lineStartX = ImGui.GetCursorPosX();
                for (int n = 0; n < _rows && addr < memSize; n++, addr++)
                {
                    ImGui.SameLine(lineStartX + cellWidth * n);

                    if (_dataEditingAddr == addr)
                    {
                        // Display text input on current byte
                        ImGui.PushID(addr);

                        int cursorPos = -1;
                        ImGuiInputTextCallback callback = (data) =>
                        {
                            int* pCursorPos = (int*)data->UserData;
                            if (ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                                *pCursorPos = data->CursorPos;
                            return 0;
                        };

                        bool dataWrite = false;
                        if (_dataEditingTakeFocus)
                        {
                            ImGui.SetKeyboardFocusHere();
                            ReplaceChars(_dataInput, FixedHex(memData[addr], 2));
                            ReplaceChars(_addrInput, FixedHex(baseDisplayAddr + addr, addrDigitsCount));
                        }

                        ImGui.PushItemWidth(ImGui.CalcTextSize("FF").X);

                        var flags = ImGuiInputTextFlags.CharsHexadecimal |
                                    ImGuiInputTextFlags.EnterReturnsTrue |
                                    ImGuiInputTextFlags.AutoSelectAll |
                                    ImGuiInputTextFlags.NoHorizontalScroll |
                                    ImGuiInputTextFlags.CallbackAlways;

                        var d = _dataInput[0].ToString();
                        if (ImGui.InputText("##data", ref d, (uint)_dataInput.Length, flags,
                                callback, (IntPtr)(&cursorPos)))
                        {
                            dataWrite = true;
                        }
                        else if (!_dataEditingTakeFocus && !ImGui.IsItemActive())
                        {
                            _dataEditingAddr = -1;
                        }

                        _dataEditingTakeFocus = false;
                        ImGui.PopItemWidth();

                        if (cursorPos >= 2)
                            dataWrite = true;

                        if (dataWrite && _allowEdits)
                        {
                            if (TryHexParse(_dataInput, out int data))
                            {
                                memData[addr] = (byte)data;
                                _dataEditingAddr++;
                                _dataEditingTakeFocus = true;
                            }
                        }

                        ImGui.PopID();
                    }
                    else
                    {
                        ImGui.Text(FixedHex(memData[addr], 2));
                        if (_allowEdits && ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            _dataEditingTakeFocus = true;
                            _dataEditingAddr = addr;
                        }
                    }
                }

                ImGui.PopStyleColor();

                ImGui.SameLine(lineStartX + cellWidth * _rows + glyphWidth * 2);

                // Draw ASCII values
                addr = line_i * _rows;
                var asciiVal = new StringBuilder(2 + _rows);
                asciiVal.Append("| ");
                for (int n = 0; n < _rows && addr < memSize; n++, addr++)
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

        // Handle keyboard navigation
        if (_dataEditingAddr >= 0 && _dataEditingAddr < memSize)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && _dataEditingAddr >= _rows)
            {
                _dataEditingAddr -= _rows;
                _dataEditingTakeFocus = true;
            }
            else if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) && _dataEditingAddr < memSize - _rows)
            {
                _dataEditingAddr += _rows;
                _dataEditingTakeFocus = true;
            }
            else if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow) && _dataEditingAddr > 0)
            {
                _dataEditingAddr -= 1;
                _dataEditingTakeFocus = true;
            }
            else if (ImGui.IsKeyPressed(ImGuiKey.RightArrow) && _dataEditingAddr < memSize - 1)
            {
                _dataEditingAddr += 1;
                _dataEditingTakeFocus = true;
            }
        }

        ImGui.Separator();

        // Controls
        ImGui.AlignTextToFramePadding();
        ImGui.PushItemWidth(50);
        //ImGui.PushAllowKeyboardFocus(true);

        int rowsBackup = _rows;
        if (ImGui.DragInt("##rows", ref _rows, 0.2f, 4, 32, "%.0f rows"))
        {
            if (_rows <= 0) _rows = 4;
        }

        //ImGui.PopAllowKeyboardFocus();
        ImGui.PopItemWidth();

        ImGui.SameLine();
        ImGui.Text(
            $"Range {FixedHex(baseDisplayAddr, addrDigitsCount)}..{FixedHex(baseDisplayAddr + memSize - 1, addrDigitsCount)}");

        ImGui.SameLine();
        ImGui.PushItemWidth(70);

        var d2 = _addrInput[0].ToString();
        if (ImGui.InputText("##addr", ref d2, (uint)_addrInput.Length,
                ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            if (TryHexParse(_addrInput, out int gotoAddr))
            {
                gotoAddr -= baseDisplayAddr;
                if (gotoAddr >= 0 && gotoAddr < memSize)
                {
                    _dataEditingAddr = gotoAddr;
                    _dataEditingTakeFocus = true;

                    // Scroll to the address
                    float scrollY = (gotoAddr / _rows) * lineHeight;
                    ImGui.SetScrollY(scrollY);
                }
            }
        }

        ImGui.PopItemWidth();

        ImGui.SameLine();
        if (ImGui.Checkbox("Allow Edits", ref _allowEdits))
        {
            if (!_allowEdits)
                _dataEditingAddr = -1;
        }

        ImGui.End();
    }

    public void Resize(int width, int height)
    {
        // No resize handling needed for ImGui windows
    }
}
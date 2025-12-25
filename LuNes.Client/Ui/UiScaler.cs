using ImGuiNET;

namespace LuNes.Client.Ui;

public static class UiScaler
{
    private static float _scale = 2.0f;
    public static float Scale
    {
        get => _scale;
        private set => SetScale(value);
    } 
    
    public static void ApplyScale()
    {
        var io = ImGui.GetIO();
        io.FontGlobalScale = 2;
        
        var style = ImGui.GetStyle();
        
        style.ScaleAllSizes(Scale);
        // Scale style elements
        /*style.WindowPadding *= Scale;
        style.WindowRounding *= Scale;
        style.FramePadding *= Scale;
        style.FrameRounding *= Scale;
        style.ItemSpacing *= Scale;
        style.ItemInnerSpacing *= Scale;
        style.CellPadding *= Scale;
        style.TouchExtraPadding *= Scale;
        style.IndentSpacing *= Scale;
        style.ScrollbarSize *= Scale;
        style.ScrollbarRounding *= Scale;
        style.GrabMinSize *= Scale;
        style.GrabRounding *= Scale;*/
        
        // Scale colors might need adjustment too
        /*style.Colors[(int)ImGuiCol.Text] *= Scale;
        style.Colors[(int)ImGuiCol.TextDisabled] *= Scale;*/
    }
    
    public static void SetScale(float scale)
    {
        _scale = Math.Clamp(scale, 0.5f, 4.0f);
        ApplyScale();
    }
}
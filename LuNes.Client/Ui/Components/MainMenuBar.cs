using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public class MainMenuBar : IComponent
{
    private readonly RomLoader _romLoader;
    private readonly Func<bool> _canLoadRom;
    private readonly Action _onStep;
    private readonly Action _onReset;
    private readonly Action _onToggleRun;
    private readonly Func<bool> _getRunState;
    private string _currentRomName = "";
    
    public bool IsVisible { get; set; } = true;
    
    public MainMenuBar(
        RomLoader romLoader,
        Func<bool> canLoadRom,
        Action onStep,
        Action onReset,
        Action onToggleRun,
        Func<bool> getRunState)
    {
        _romLoader = romLoader;
        _canLoadRom = canLoadRom;
        _onStep = onStep;
        _onReset = onReset;
        _onToggleRun = onToggleRun;
        _getRunState = getRunState;
    }
    
    public void Update(float deltaTime) { }
    
    public void Draw()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Load ROM", "Ctrl+O", false, _canLoadRom()))
                {
                    _romLoader.ShowFileDialog = true;
                }
                
                ImGui.Separator();
                
                if (ImGui.MenuItem("Exit", "Alt+F4"))
                {
                    Environment.Exit(0);
                }
                
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Emulation"))
            {
                if (ImGui.MenuItem("Step", "Space", false, !_getRunState()))
                {
                    _onStep();
                }
                
                if (ImGui.MenuItem("Reset", "R"))
                {
                    _onReset();
                }
                
                if (ImGui.MenuItem(_getRunState() ? "Pause" : "Run", "A"))
                {
                    _onToggleRun();
                }
                
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("RAM Viewer", "", true);
                ImGui.MenuItem("Disassembly", "", true);
                ImGui.MenuItem("CPU Status", "", true);
                ImGui.MenuItem("Devices", "", true);
                ImGui.EndMenu();
            }
            
            ImGui.SameLine(ImGui.GetWindowWidth() - 200);
            ImGui.Text(_getRunState() ? "RUNNING" : "PAUSED");
            
            if (!string.IsNullOrEmpty(_currentRomName))
            {
                ImGui.SameLine();
                ImGui.TextDisabled($"| {_currentRomName}");
            }
            
            ImGui.EndMainMenuBar();
        }
    }
    
    public void SetCurrentRom(string romName)
    {
        _currentRomName = Path.GetFileName(romName);
    }
    
    public void Resize(int width, int height) { }
}
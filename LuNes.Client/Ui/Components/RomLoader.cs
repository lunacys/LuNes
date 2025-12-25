using ImGuiNET;

namespace LuNes.Client.Ui.Components;

public class RomLoader : IComponent
{
    private readonly Func<byte[], bool> _onRomLoaded;
    private string _currentDirectory = "Content/ROMs";
    private string[] _romFiles = Array.Empty<string>();
    private string _selectedFile = "";
    public bool ShowFileDialog;

    public bool IsVisible { get; set; } = true;

    public RomLoader(Func<byte[], bool> onRomLoaded)
    {
        _onRomLoaded = onRomLoaded;
        RefreshFileList();
    }

    private void RefreshFileList()
    {
        if (Directory.Exists(_currentDirectory))
        {
            _romFiles = Directory.GetFiles(_currentDirectory, "*.bin", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(_currentDirectory, f))
                .ToArray();
        }
    }

    public void Update(float deltaTime)
    {
    }

    public void Draw()
    {
        if (ShowFileDialog)
        {
            ImGui.OpenPopup("Load ROM File");
            ShowFileDialog = false;
        }

        if (ImGui.BeginPopupModal("Load ROM File"))
        {
            if (ImGui.Button("Refresh"))
            {
                RefreshFileList();
            }

            ImGui.SameLine();
            ImGui.Text($"Directory: {_currentDirectory}");

            ImGui.BeginChild("##FileList", new System.Numerics.Vector2(400, 300));

            foreach (var file in _romFiles)
            {
                if (ImGui.Selectable(file, _selectedFile == file))
                {
                    _selectedFile = file;
                }
            }

            ImGui.EndChild();

            if (ImGui.Button("Load") && !string.IsNullOrEmpty(_selectedFile))
            {
                string fullPath = Path.Combine(_currentDirectory, _selectedFile);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        var romData = File.ReadAllBytes(fullPath);
                        _onRomLoaded?.Invoke(romData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading ROM: {ex.Message}");
                    }
                }

                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    public void Resize(int width, int height)
    {
    }
}
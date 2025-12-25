using ImGuiNET;
using LuNes.Client.Debugging;

namespace LuNes.Client.Ui.Components;

public class Profiler : IComponent
{
    private enum SortMode
    {
        Name,
        LastTime,
        MeanTime,
        MinTime,
        MaxTime,
        StdDev,
        SampleCount
    }

    private SortMode _currentSort = SortMode.Name;
    private bool _sortAscending = true;
    private bool _paused;
    private bool _showPlot;
    private string _filterText = "";
    private int _plotInstanceId = -1;
    
    private Dictionary<string, PlotData> _plotData = new();
    private const int PlotHistoryLength = 100;
    
    public bool IsVisible { get; set; } = true;

    private class PlotData
    {
        public float[] Samples = new float[PlotHistoryLength];
        public float MaxValue = 1f;
        public int InstanceId;
        public int SampleCount = 0;
        public int WriteIndex = 0;
    }

    private BackgroundEmulator _emulator;

    public Profiler(BackgroundEmulator emulator)
    {
        _emulator = emulator;
    }

    public void Update(float deltaTime)
    {
        if (_paused) return;
        
        // Update plot data
        var snapshot = TimeManager.TimeData;
        foreach (var entry in snapshot)
        {
            if (!_plotData.TryGetValue(entry.Key, out var plotData))
            {
                plotData = new PlotData
                {
                    InstanceId = Interlocked.Increment(ref _plotInstanceId)
                };
                _plotData[entry.Key] = plotData;
            }

            var data = entry.Value;
            //plotData.Samples.Add((float)data.Mean);
            plotData.Samples[_plotInstanceId % PlotHistoryLength] = (float) data.Mean;
            plotData.WriteIndex = (plotData.WriteIndex + 1) % PlotHistoryLength;
            plotData.SampleCount = Math.Min(plotData.SampleCount + 1, PlotHistoryLength);
            // if (plotData.Samples.Count > PlotHistoryLength)
            //     plotData.Samples.RemoveAt(0);
            
            // Update max value for scaling
            float currentMax = plotData.Samples.Max();
            plotData.MaxValue = Math.Max(plotData.MaxValue, currentMax * 1.1f);
        }
        
        // Clean up old plot data
        var keysToRemove = _plotData.Keys.Where(k => !snapshot.ContainsKey(k)).ToList();
        foreach (var key in keysToRemove)
            _plotData.Remove(key);
    }

    public void Draw()
    {
        ImGui.Begin("Profiler", ImGuiWindowFlags.NoScrollbar);
        
        DrawControls();
        
        ImGui.Separator();
        
        DrawStatsTable();
        
        if (_showPlot)
        {
            ImGui.Separator();
            DrawPlots();
        }
        
        ImGui.End();
    }

    private void DrawControls()
    {
        if (ImGui.Button(_paused ? "Resume" : "Pause"))
            _paused = !_paused;
        
        ImGui.SameLine();
        if (ImGui.Button("Clear"))
            TimeManager.Clear();
        
        ImGui.SameLine();
        ImGui.Checkbox("Show Plots", ref _showPlot);
        
        ImGui.SameLine();
        ImGui.PushItemWidth(200);
        ImGui.InputText("Filter", ref _filterText, 256);
        ImGui.PopItemWidth();
        
        ImGui.SameLine();
        ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), 
            $"FPS: {_emulator.CurrentFps:F1}");
    }

    private void DrawStatsTable()
    {
        var timeData = TimeManager.TimeData;
        var filteredEntries = timeData
            .Where(kv => string.IsNullOrEmpty(_filterText) || 
                        kv.Key.Contains(_filterText, StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Value.GetSnapshot())
            .ToList();

        var sortedEntries = _currentSort switch
        {
            SortMode.Name => filteredEntries.OrderBy(e => e.Context),
            SortMode.LastTime => filteredEntries.OrderBy(e => e.LastTime),
            SortMode.MeanTime => filteredEntries.OrderBy(e => e.Mean),
            SortMode.MinTime => filteredEntries.OrderBy(e => e.Min),
            SortMode.MaxTime => filteredEntries.OrderBy(e => e.Max),
            SortMode.StdDev => filteredEntries.OrderBy(e => e.StandardDeviation),
            SortMode.SampleCount => filteredEntries.OrderBy(e => e.SampleCount),
            _ => filteredEntries.OrderBy(e => e.Context)
        };

        //if (!_sortAscending)
        //    sortedEntries = sortedEntries.Reverse().Order();
        
        ImGui.BeginChild("##ProfilerTable", new System.Numerics.Vector2(0, 300), 
            ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
        
        ImGui.BeginTable("ProfilerTable", 7, 
            ImGuiTableFlags.Sortable | 
            ImGuiTableFlags.Resizable | 
            ImGuiTableFlags.Reorderable | 
            ImGuiTableFlags.RowBg | 
            ImGuiTableFlags.Borders);
        
        ImGui.TableSetupColumn("Context", ImGuiTableColumnFlags.DefaultSort | 
            ImGuiTableColumnFlags.WidthStretch, 0.3f);
        ImGui.TableSetupColumn("Last (ms)", ImGuiTableColumnFlags.WidthFixed, 80);
        ImGui.TableSetupColumn("Mean (ms)", ImGuiTableColumnFlags.WidthFixed, 80);
        ImGui.TableSetupColumn("Min (ms)", ImGuiTableColumnFlags.WidthFixed, 80);
        ImGui.TableSetupColumn("Max (ms)", ImGuiTableColumnFlags.WidthFixed, 80);
        ImGui.TableSetupColumn("StdDev (ms)", ImGuiTableColumnFlags.WidthFixed, 80);
        ImGui.TableSetupColumn("Samples", ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableHeadersRow();
        
        foreach (var entry in sortedEntries)
        {
            ImGui.TableNextRow();
            
            ImGui.TableNextColumn();
            ImGui.Text(entry.Context);
            
            ImGui.TableNextColumn();
            ColorTimeCell(entry.LastTime.TotalMilliseconds, entry.Mean, entry.StandardDeviation);
            ImGui.Text($"{entry.LastTime.TotalMilliseconds:F3}");
            ImGui.PopStyleColor();
            
            ImGui.TableNextColumn();
            ColorTimeCell(entry.Mean, entry.Mean, entry.StandardDeviation);
            ImGui.Text($"{entry.Mean:F3}");
            ImGui.PopStyleColor();
            
            ImGui.TableNextColumn();
            ColorTimeCell(entry.Min, entry.Mean, entry.StandardDeviation);
            ImGui.Text($"{entry.Min:F3}");
            ImGui.PopStyleColor();
            
            ImGui.TableNextColumn();
            ColorTimeCell(entry.Max, entry.Mean, entry.StandardDeviation);
            ImGui.Text($"{entry.Max:F3}");
            ImGui.PopStyleColor();
            
            ImGui.TableNextColumn();
            ColorStdDevCell(entry.StandardDeviation, entry.Mean);
            ImGui.Text($"{entry.StandardDeviation:F3}");
            ImGui.PopStyleColor();
            
            ImGui.TableNextColumn();
            ImGui.Text($"{entry.SampleCount}");
        }
        
        ImGui.EndTable();
        ImGui.EndChild();
    }

    private void DrawPlots()
    {
        if (_plotData.Count == 0) return;
        
        ImGui.Text("Performance Trends:");
        
        float availableWidth = ImGui.GetContentRegionAvail().X;
        float plotWidth = availableWidth;
        float plotHeight = 100;
        
        int plotIndex = 0;
        foreach (var kvp in _plotData)
        {
            ImGui.PushID($"Plot_{kvp.Key}_{kvp.Value.InstanceId}");
            
            ImGui.Text(kvp.Key);
            ImGui.SameLine();
            ImGui.TextDisabled($"Max: {kvp.Value.MaxValue:F2}ms");
            
            if (kvp.Value.SampleCount > 0)
            {
                int startIdx = kvp.Value.WriteIndex - kvp.Value.SampleCount;
                if (startIdx < 0) startIdx += PlotHistoryLength;
                
                float[] orderedSamples = new float[kvp.Value.SampleCount];
                for (int i = 0; i < kvp.Value.SampleCount; i++)
                {
                    orderedSamples[i] = kvp.Value.Samples[(startIdx + i) % PlotHistoryLength];
                }
                
                if (orderedSamples.Length > 0)
                {
                    ImGui.PlotLines($"##PlotLines_{kvp.Key}_{kvp.Value.InstanceId}", 
                        ref orderedSamples[0], orderedSamples.Length, 0, 
                        string.Empty, 0f, kvp.Value.MaxValue, 
                        new System.Numerics.Vector2(plotWidth, plotHeight));
                }
            }
            else
            {
                ImGui.InvisibleButton($"##EmptyPlot_{kvp.Key}", new System.Numerics.Vector2(plotWidth, plotHeight));
            }
            
            ImGui.PopID();
            
            if (plotIndex < _plotData.Count - 1)
                ImGui.Spacing();
            
            plotIndex++;
        }
    }

    private void ColorTimeCell(double value, double mean, double stdDev)
    {
        double zScore = (value - mean) / (stdDev + 0.001);
        
        if (Math.Abs(zScore) < 1)
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 1, 1, 1));
        else if (Math.Abs(zScore) < 2)
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 1, 0, 1));
        else
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
    }

    private void ColorStdDevCell(double stdDev, double mean)
    {
        double cv = (mean > 0) ? stdDev / mean : 0;
        
        if (cv < 0.1)
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 1, 0, 1));
        else if (cv < 0.3)
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 1, 0, 1));
        else
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
    }

    public void Resize(int width, int height) { }
}
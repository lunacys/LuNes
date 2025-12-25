using LuNes.Client.Debugging;
using LuNes.Client.Ui;
using LuNes.Client.Ui.Components;
using Raylib_cs;

namespace LuNes.Client;

public class Startup : IDisposable
{
    private Font _mainFont;

    private Computer _computer;
    private ComponentManager _componentManager;
    private BackgroundEmulator _emulator;

    private bool _isDisposed;

    public bool IsEmulationRunning => _emulator.RunEmulation;

    public Startup()
    {
        _mainFont = Raylib.LoadFontEx(Path.Combine("Content", "Fonts", "FiraCode-Regular.ttf"), 32, null, 0);

        var initialRom = LoadRom("Content/ROMs/tests/1_basic_instructions.bin");
        _computer = new Computer(initialRom);
        _emulator = new BackgroundEmulator(_computer);

        _componentManager = new ComponentManager();

        _componentManager.Register(new EmulatorSettings(_emulator));
        _componentManager.Register(new MemoryEditor(_computer.Bus));
        //ramViewer = _componentManager.Register(new RamViewer(_computer.Bus, _mainFont));
        _componentManager.Register(new CpuStatus(_computer.Bus, _mainFont));
        _componentManager.Register(new DeviceViewer(_computer, _mainFont));
        var dasm = _componentManager.Register(new DisassemblyViewer(_computer.Bus));
        _componentManager.Register(new Profiler(_emulator));
        var romLoader = _componentManager.Register(new RomLoader(bytes =>
        {
            _computer.Rom.Load(bytes);
            dasm.UpdateDisassembly();
            _computer.Bus.Reset();
            return true;
        }));

        _componentManager.Register(new MainMenuBar(
            romLoader,
            () => true,
            () => _emulator.Step(),
            () => _emulator.Post(() => _computer.Bus.Reset()),
            () => _emulator.ToggleEmulation(),
            () => IsEmulationRunning));

        dasm.UpdateDisassembly();

        _emulator.Start();
    }

    ~Startup()
    {
        Dispose(false);
    }

    public void Update()
    {
        var dt = Raylib.GetFrameTime();

        // Single step
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            TimeManager.TimeAction(() => _emulator.Step(), "Emulator.Step");
        }

        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            _emulator.Post(() =>
                TimeManager.TimeAction(() => _computer.Bus.Reset(), "Computer.Reset")
            );
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            _emulator.Post(() =>
                TimeManager.TimeAction(() => _computer.Bus.Cpu.Irq(), "Computer.Cpu.Irq")
            );
        }

        if (Raylib.IsKeyPressed(KeyboardKey.W))
        {
            _emulator.Post(() =>
                TimeManager.TimeAction(() => _computer.Bus.Cpu.Nmi(), "Computer.Cpu.Nmi")
            );
        }

        if (Raylib.IsKeyPressed(KeyboardKey.A))
            _emulator.ToggleEmulation();

        TimeManager.TimeAction(() => _componentManager.Update(dt), "ComponentManager.Update");
    }

    public void Draw()
    {
        TimeManager.TimeAction(() => _componentManager.Draw(), "ComponentManager.Draw");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _emulator?.Dispose();
            }

            _isDisposed = true;
        }
    }

    private byte[] LoadRom(string filePath)
    {
        try
        {
            return File.ReadAllBytes(filePath);
        }
        catch
        {
            Console.WriteLine($"Failed to load ROM: {filePath}");
            return new byte[32768];
        }
    }
}
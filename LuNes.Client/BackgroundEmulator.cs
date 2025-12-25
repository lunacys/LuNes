using System.Diagnostics;
using System.Threading.Channels;
using LuNes.Client.Debugging;

namespace LuNes.Client;

public class BackgroundEmulator : IDisposable
{
    private Channel<Action> _emulationChannel = null!;
    private CancellationTokenSource? _emulationCts;
    private Thread? _emulationThread;
    private bool _isDisposed;

    private Computer _computer;

    public bool RunEmulation;

    // In Hz (cycles per second)
    private double _targetClockSpeed = 60;

    public double TargetClockSpeed
    {
        get => _targetClockSpeed;
        set => _targetClockSpeed = Math.Max(0.1, value);
    }

    public double MsPerCycle => 1000.0 / _targetClockSpeed;

    public double ActualClockSpeed { get; private set; }
    private int _cycleCount;
    private double _speedTime;

    public double CurrentFps { get; private set; }
    private int _frameCount;
    private double _fpsTime;

    public BackgroundEmulator(Computer computer)
    {
        _computer = computer;
    }

    ~BackgroundEmulator()
    {
        Dispose(false);
    }

    public void Start()
    {
        if (_emulationCts != null && !_emulationCts.IsCancellationRequested)
            return;

        _emulationChannel = Channel.CreateUnbounded<Action>();
        _emulationCts = new CancellationTokenSource();

        _emulationThread = new Thread(() => Emulate(_emulationCts.Token))
        {
            IsBackground = true,
            Name = "Emulation",
            Priority = ThreadPriority.AboveNormal
        };
        _emulationThread.Start();
    }

    public void Stop()
    {
        RunEmulation = false;
        _emulationCts?.Cancel();

        if (_emulationThread != null && _emulationThread.IsAlive)
        {
            var isJoined = _emulationThread.Join(TimeSpan.FromSeconds(1));
            if (!isJoined)
            {
                Console.WriteLine("WARNING: Emulation thread did not stop gracefully");
            }
        }
    }

    public void Post(Action action)
    {
        _emulationChannel.Writer.TryWrite(action);
    }

    public void ToggleEmulation()
    {
        RunEmulation = !RunEmulation;
    }

    public void Step()
    {
        TimeManager.TimeAction(() =>
        {
            do
            {
                _computer.Bus.Clock();
            } while (!_computer.Bus.Cpu.IsComplete());
        }, "Emulator.Clock");
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
                Stop();
                _emulationCts?.Dispose();
            }

            _isDisposed = true;
        }
    }

    private async void Emulate(CancellationToken ct)
    {
        try
        {
            var reader = _emulationChannel.Reader;
            var sw = new Stopwatch();
            double accumulatedTime = 0;
            const double maxAccumulatedTime = 1000.0;

            sw.Start();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var realTimeElapsed = sw.ElapsedMilliseconds;
                    sw.Restart();

                    accumulatedTime += realTimeElapsed;
                    accumulatedTime = Math.Min(accumulatedTime, maxAccumulatedTime);

                    while (reader.TryRead(out var action))
                        action();

                    double msPerCycle = 1000.0 / _targetClockSpeed;

                    int cyclesThisIteration = 0;
                    while (accumulatedTime >= msPerCycle && RunEmulation)
                    {
                        TimeManager.TimeAction(() => _computer.Bus.Clock(), "Emulator.Clock");

                        cyclesThisIteration++;
                        accumulatedTime -= msPerCycle;

                        _cycleCount++;

                        if (cyclesThisIteration > 100000)
                        {
                            break;
                        }
                    }

                    // Update actual clock speed
                    _speedTime += realTimeElapsed;
                    if (_speedTime >= 1000)
                    {
                        ActualClockSpeed = _cycleCount * 1000.0 / _speedTime;
                        _cycleCount = 0;
                        _speedTime = 0;
                    }

                    // Update FPS
                    _fpsTime += realTimeElapsed;
                    _frameCount++;
                    if (_fpsTime >= 1000)
                    {
                        CurrentFps = _frameCount * 1000.0 / _fpsTime;
                        _frameCount = 0;
                        _fpsTime = 0;
                    }

                    await Task.Delay(1, ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in emulation loop: {ex.Message}");
                    await Task.Delay(100, ct).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fatal error in emulation thread: {e.Message}");
        }
        finally
        {
            Console.WriteLine("Emulation thread stopped");
        }
    }
}
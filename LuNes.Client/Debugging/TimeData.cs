namespace LuNes.Client.Debugging;

public sealed class TimeData
{
    private readonly double[] _samples;
    private readonly int _recalculateInterval;
    private int _sampleCount;
    private int _nextIndex;
    private int _recalculateCounter;

    private readonly Lock _lock = new();

    public string Context { get; }
    public TimeSpan LastTime { get; private set; }
    public int Capacity => _samples.Length;

    public double Mean { get; private set; }
    public double Min { get; private set; } = double.MaxValue;
    public double Max { get; private set; } = double.MinValue;
    public double StandardDeviation { get; private set; }

    public TimeData(string context, int sampleSize = 64, int recalculateEvery = 16)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _samples = new double[sampleSize];
        _recalculateInterval = Math.Max(1, recalculateEvery);
    }

    public void Update(TimeSpan duration)
    {
        lock (_lock)
        {
            LastTime = duration;
            var milliseconds = duration.TotalMilliseconds;

            if (milliseconds < Min) Min = milliseconds;
            if (milliseconds > Max) Max = milliseconds;

            _samples[_nextIndex] = milliseconds;
            _nextIndex = (_nextIndex + 1) % _samples.Length;

            if (_sampleCount < _samples.Length)
                _sampleCount++;

            if (++_recalculateCounter >= _recalculateInterval)
            {
                Recalculate();
                _recalculateCounter = 0;
            }
        }
    }

    private void Recalculate()
    {
        if (_sampleCount == 0)
        {
            Mean = 0;
            StandardDeviation = 0;
            return;
        }

        double sum = 0;
        double sumSquares = 0;

        for (int i = 0; i < _sampleCount; i++)
        {
            double value = _samples[i];
            sum += value;
            sumSquares += value * value;
        }

        Mean = sum / _sampleCount;
        double variance = (sumSquares / _sampleCount) - (Mean * Mean);
        StandardDeviation = Math.Sqrt(Math.Max(0, variance));
    }

    public void Clear()
    {
        lock (_lock)
        {
            Array.Clear(_samples, 0, _samples.Length);
            _sampleCount = 0;
            _nextIndex = 0;
            _recalculateCounter = 0;
            Mean = 0;
            Min = double.MaxValue;
            Max = double.MinValue;
            StandardDeviation = 0;
            LastTime = TimeSpan.Zero;
        }
    }

    public ReadOnlySpan<double> GetSamples()
    {
        lock (_lock)
        {
            return new ReadOnlySpan<double>(_samples, 0, _sampleCount);
        }
    }

    public TimeDataSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            return new TimeDataSnapshot(
                Context,
                LastTime,
                Mean,
                Min,
                Max,
                StandardDeviation,
                _sampleCount
            );
        }
    }

    public override string ToString()
    {
        var snapshot = GetSnapshot();
        return $"{snapshot.Context} | Last: {snapshot.LastTime.TotalMilliseconds:F3}ms | " +
               $"Mean: {snapshot.Mean:F3}ms | Min: {snapshot.Min:F3}ms | Max: {snapshot.Max:F3}ms | " +
               $"StdDev: {snapshot.StandardDeviation:F3}ms | Samples: {snapshot.SampleCount}";
    }
}

public readonly struct TimeDataSnapshot
{
    public readonly string Context;
    public readonly TimeSpan LastTime;
    public readonly double Mean;
    public readonly double Min;
    public readonly double Max;
    public readonly double StandardDeviation;
    public readonly int SampleCount;

    public TimeDataSnapshot(
        string context,
        TimeSpan lastTime,
        double mean,
        double min,
        double max,
        double standardDeviation,
        int sampleCount)
    {
        Context = context;
        LastTime = lastTime;
        Mean = mean;
        Min = min;
        Max = max;
        StandardDeviation = standardDeviation;
        SampleCount = sampleCount;
    }
}
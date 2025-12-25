using System.Collections.Concurrent;
using System.Diagnostics;

namespace LuNes.Client.Debugging;

public static class TimeManager
{
    private static readonly ConcurrentDictionary<string, TimeData> _timeData = new();

    public static IReadOnlyDictionary<string, TimeData> TimeData => _timeData;

    public static void AddSample(string context, TimeSpan duration)
    {
        var timeData = _timeData.GetOrAdd(context, key => new TimeData(key));
        timeData.Update(duration);
    }

    public static TimeSpan TimeAction(Action action, string? context = null)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();

        if (!string.IsNullOrEmpty(context))
            AddSample(context, sw.Elapsed);

        return sw.Elapsed;
    }

    public static async Task<TimeSpan> TimeActionAsync(Func<Task> asyncAction, string? context = null)
    {
        var sw = Stopwatch.StartNew();
        await asyncAction();
        sw.Stop();

        if (!string.IsNullOrEmpty(context))
            AddSample(context, sw.Elapsed);

        return sw.Elapsed;
    }

    public static T TimeFunc<T>(Func<T> func, out TimeSpan elapsed, string? context = null)
    {
        var sw = Stopwatch.StartNew();
        var result = func();
        sw.Stop();
        elapsed = sw.Elapsed;

        if (!string.IsNullOrEmpty(context))
            AddSample(context, elapsed);

        return result;
    }

    public static (T Result, TimeSpan Elapsed) TimeFunc<T>(Func<T> func, string? context = null)
    {
        var result = TimeFunc(func, out var elapsed, context);
        return (result, elapsed);
    }

    public static async Task<(T Result, TimeSpan Elapsed)> TimeFuncAsync<T>(Func<Task<T>> asyncFunc,
        string? context = null)
    {
        var sw = Stopwatch.StartNew();
        var result = await asyncFunc();
        sw.Stop();
        var elapsed = sw.Elapsed;

        if (!string.IsNullOrEmpty(context))
            AddSample(context, elapsed);

        return (result, elapsed);
    }

    public static TimeScope Scope(string context)
    {
        return new TimeScope(context);
    }

    public static void Clear()
    {
        foreach (var timeData in _timeData.Values)
        {
            timeData.Clear();
        }
    }

    public static void RemoveContext(string context)
    {
        _timeData.TryRemove(context, out _);
    }

    public readonly ref struct TimeScope : IDisposable
    {
        private readonly string _context;
        private readonly long _startTimestamp;

        public TimeScope(string context)
        {
            _context = context;
            _startTimestamp = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            var elapsed = Stopwatch.GetElapsedTime(_startTimestamp);
            AddSample(_context, elapsed);
        }
    }
}
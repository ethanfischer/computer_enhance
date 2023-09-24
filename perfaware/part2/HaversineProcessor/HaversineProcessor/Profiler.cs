#define PROFILING

using JsonGenerator;
using static SMXGo.Scripts.Other.SMXProfiler;

namespace SMXGo.Scripts.Other;

#if PROFILING
public struct ProfileAnchor
{
    public ulong TSCElapsedInclusive; //Time including children
    public ulong TCSElapsedExclusive;
    public ulong HitCount;
    public string Label;
    public int ProcessedByteCount;
}
public record Profiler
{
    public readonly ProfileAnchor[] Anchors = new ProfileAnchor[4096];
    public ulong StartTSC;
    public ulong EndTSC;
}
public record ProfileBlock : IDisposable
{
    readonly string _label;
    readonly ulong _startTsc;
    readonly ulong _oldTscElapsedInclusive;
    readonly uint _parentIndex;
    readonly uint _anchorIndex;

    public ProfileBlock(string label, uint anchorIndex, int byteCount)
    {
        _parentIndex = ProfilerParent;

        _anchorIndex = anchorIndex;
        _label = label;

        var anchor = GlobalProfiler.Anchors[_anchorIndex];
        _oldTscElapsedInclusive = anchor.TSCElapsedInclusive;
        anchor.ProcessedByteCount += byteCount;
        GlobalProfiler.Anchors[_anchorIndex] = anchor;

        ProfilerParent = _anchorIndex;
        _startTsc = TimerService.ReadCPUTimer();
    }

    public void Dispose()
    {
        var elapsed = TimerService.ReadCPUTimer() - _startTsc;
        ProfilerParent = _parentIndex;

        var anchor = GlobalProfiler.Anchors[_anchorIndex];
        anchor.TCSElapsedExclusive += elapsed;
        anchor.TSCElapsedInclusive = _oldTscElapsedInclusive + elapsed;
        anchor.HitCount++;
        anchor.Label = _label;
        GlobalProfiler.Anchors[_anchorIndex] = anchor;

        var parent = GlobalProfiler.Anchors[_parentIndex];
        parent.TCSElapsedExclusive -= elapsed;
        GlobalProfiler.Anchors[_parentIndex] = parent;
    }
}
public class ProfilerReport
{
    public ulong TotalCpuElapsed { get; set; }
    public ulong CpuFrequency { get; set; }
    public ProfileAnchor[] Anchors { get; set; }
}
public static class SMXProfiler
{
    public static readonly Profiler GlobalProfiler = new();
    public static uint ProfilerParent;
    private static uint _counter;
    private static readonly Dictionary<string, uint> _labelToIndexMap = new();

    public static ProfileBlock TimeBlock(string label, int byteCount = 0)
    {
        if (!_labelToIndexMap.ContainsKey(label))
        {
            _labelToIndexMap[label] = ++_counter;
        }

        return new ProfileBlock(label, _labelToIndexMap[label], byteCount);
    }

    static void PrintTimeElapsed(ulong totalTSCElapsed, ProfileAnchor anchor, float cpuFrequency)
    {
        var percent = 100.0 * ((float)anchor.TCSElapsedExclusive / totalTSCElapsed);
        var logMessage = $"{anchor.Label}[{anchor.HitCount}]: {anchor.TCSElapsedExclusive} ({percent:F2}%)";
        if (anchor.TCSElapsedExclusive != anchor.TSCElapsedInclusive)
        {
            var percentWithChildren = 100.0 * ((float)anchor.TSCElapsedInclusive / totalTSCElapsed);
            logMessage += $" ({percentWithChildren:F2}% w/children)";
        }

        if (anchor.ProcessedByteCount > 0)
        {
            var mebibyte = 1024.0f * 1024.0f;
            var gibibyte = mebibyte * 1024.0f;

            var seconds = anchor.TSCElapsedInclusive / (double)cpuFrequency;
            var bytesPerSecond = (ulong)anchor.ProcessedByteCount / seconds;
            var mebibytes = (ulong)anchor.ProcessedByteCount / (ulong)mebibyte;
            var gibibytesPerSecond = bytesPerSecond / gibibyte;

            logMessage += $"  %{mebibytes:F2}MiB at %{gibibytesPerSecond:F2}GiB/s";
        }

        Log($"{logMessage}");
    }

    public static void BeginProfile()
    {
        GlobalProfiler.StartTSC = TimerService.ReadCPUTimer();
    }

    public static ProfilerReport EndAndPrintProfile()
    {
        GlobalProfiler.EndTSC = TimerService.ReadCPUTimer();

        var result = new ProfilerReport
        {
            CpuFrequency = TimerService.EstimateCPUTimerFreq(),
            TotalCpuElapsed = GlobalProfiler.EndTSC - GlobalProfiler.StartTSC
        };

        if (result.CpuFrequency > 0)
        {
            Log($"Total time: {1000.0 * (float)result.TotalCpuElapsed / result.CpuFrequency:F4}ms (CPU freq {result.CpuFrequency})\n");
        }

        foreach (var anchor in GlobalProfiler.Anchors)
        {
            if (anchor.TSCElapsedInclusive != 0)
            {
                PrintTimeElapsed(result.TotalCpuElapsed, anchor, result.CpuFrequency);
            }
        }

        result.Anchors = GlobalProfiler.Anchors;
        return result;
    }

    private static void Log(string message)
    {
        //Console.WriteLine(message); //TODO: just disabling log for now since I just want repition testers logs
    }
}
#else
    public record ProfileBlock : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public static class SMXProfiler
    {
        private static ulong _startTsc;
        private static ulong _endTsc;

        public static ProfileBlock TimeBlock(string label)
        {
            return null;
        }

        public static void BeginProfile()
        {
            _startTsc = TimerService.ReadCPUTimer();
        }

        public static void EndAndPrintProfile()
        {
            _endTsc = TimerService.ReadCPUTimer();
            var cpuFrequency = TimerService.EstimateCPUTimerFreq();

            var totalCpuElapsed = _endTsc - _startTsc;

            if (cpuFrequency > 0)
            {
                Log($"Total time: {1000.0 * (float)totalCpuElapsed / cpuFrequency:F4}ms (CPU freq {cpuFrequency})\n");
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
#endif
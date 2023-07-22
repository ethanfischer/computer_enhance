// #define PROFILING

using JsonGenerator;
using static SMXGo.Scripts.Other.SMXProfiler;

namespace SMXGo.Scripts.Other
{
    public struct ProfileAnchor
    {
        public ulong TSCElapsedInclusive; //Time including children
        public ulong TCSElapsedExclusive;
        public ulong HitCount;
        public string Label;
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

#if PROFILING
        public ProfileBlock(string label, uint anchorIndex)
        {
            _parentIndex = ProfilerParent;

            _anchorIndex = anchorIndex;
            _label = label;

            var anchor = GlobalProfiler.Anchors[_anchorIndex];
            _oldTscElapsedInclusive = anchor.TSCElapsedInclusive;

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
#else
        public ProfileBlock(string label, uint anchorIndex)
        {
        }
        
        public void Dispose()
        {
        }
#endif
    }

    public static class SMXProfiler
    {
        public static readonly Profiler GlobalProfiler = new();
        public static uint ProfilerParent;
        private static uint _counter;
        private static readonly Dictionary<string, uint> _labelToIndexMap = new();

#if PROFILING
        public static ProfileBlock TimeBlock(string label)
        {
            if (!_labelToIndexMap.ContainsKey(label))
            {
                _labelToIndexMap[label] = ++_counter;
            }

            return new ProfileBlock(label, _labelToIndexMap[label]);
        }
#else
        private static readonly ProfileBlock _dummyBlock = new("", 0);
        public static ProfileBlock TimeBlock(string label)
        {
            return _dummyBlock;
        }
#endif


        static void PrintTimeElapsed(ulong totalTSCElapsed, ProfileAnchor anchor)
        {
            var percent = 100.0 * ((float)anchor.TCSElapsedExclusive / totalTSCElapsed);
            var logMessage = $"{anchor.Label}[{anchor.HitCount}]: {anchor.TCSElapsedExclusive} ({percent:F2}%)";
            if (anchor.TCSElapsedExclusive != anchor.TSCElapsedInclusive)
            {
                var percentWithChildren = 100.0 * ((float)anchor.TSCElapsedInclusive / totalTSCElapsed);
                logMessage += $" ({percentWithChildren:F2}% w/children)";
            }

            Log($"{logMessage}");
        }

        public static void BeginProfile()
        {
            GlobalProfiler.StartTSC = TimerService.ReadCPUTimer();
        }

        public static void EndAndPrintProfile()
        {
            GlobalProfiler.EndTSC = TimerService.ReadCPUTimer();
            var cpuFrequency = TimerService.EstimateCPUTimerFreq();

            var totalCpuElapsed = GlobalProfiler.EndTSC - GlobalProfiler.StartTSC;

            if (cpuFrequency > 0)
            {
                Log($"Total time: {1000.0 * (float)totalCpuElapsed / cpuFrequency:F4}ms (CPU freq {cpuFrequency})\n");
            }

            foreach (var anchor in GlobalProfiler.Anchors)
            {
                if (anchor.TSCElapsedInclusive != 0)
                {
                    PrintTimeElapsed(totalCpuElapsed, anchor);
                }
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
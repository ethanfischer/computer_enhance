using JsonGenerator;
using static SMXGo.Scripts.Other.SMXProfiler;

namespace SMXGo.Scripts.Other
{
    public struct ProfileAnchor
    {
        public ulong TSCElapsed;
        public ulong TSCElapsedChildren;
        public ulong TSCElapsedAtRoot;
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
        readonly ulong _oldTscElapsedAtRoot;
        readonly uint _parentIndex;
        readonly uint _anchorIndex;

        public ProfileBlock(string label, uint anchorIndex)
        {
            _parentIndex = ProfilerParent;

            _anchorIndex = anchorIndex;
            _label = label;
            
            var anchor = GlobalProfiler.Anchors[_anchorIndex];
            _oldTscElapsedAtRoot = anchor.TSCElapsedAtRoot;

            ProfilerParent = _anchorIndex;
            _startTsc = TimerService.ReadCPUTimer();
        }

        public void Dispose()
        {
            var elapsed = TimerService.ReadCPUTimer() - _startTsc;
            ProfilerParent = _parentIndex;

            var parent = GlobalProfiler.Anchors[_parentIndex];
            parent.TSCElapsedChildren += elapsed;
            GlobalProfiler.Anchors[_parentIndex] = parent;
            
            var anchor = GlobalProfiler.Anchors[_anchorIndex];
            anchor.TSCElapsedAtRoot = _oldTscElapsedAtRoot + elapsed;
            anchor.TSCElapsed = elapsed;
            anchor.HitCount++;
            anchor.Label = _label;
            GlobalProfiler.Anchors[_anchorIndex] = anchor;
        }
    }

    public static class SMXProfiler
    {
        public static readonly Profiler GlobalProfiler = new();
        public static uint ProfilerParent;
        private static uint _counter;
        private static readonly Dictionary<string, uint> _labelToIndexMap = new();

        public static ProfileBlock TimeBlock(string label)
        {
            if (!_labelToIndexMap.ContainsKey(label))
            {
                _labelToIndexMap[label] = ++_counter;
            }

            return new ProfileBlock(label, _labelToIndexMap[label]);
        }

        static void PrintTimeElapsed(ulong totalTSCElapsed, ProfileAnchor anchor)
        {
            var elapsed = anchor.TSCElapsed - anchor.TSCElapsedChildren;
            var percent = 100.0 * ((float)elapsed / totalTSCElapsed);
            var logMessage = $"{anchor.Label}[{anchor.HitCount}]: {elapsed} ({percent:F2}%)";
            if (anchor.TSCElapsedChildren > 0)
            {
                var percentWithChildren = 100.0 * ((float)anchor.TSCElapsed / totalTSCElapsed);
                logMessage += $" ({percentWithChildren:F2}% w/children)";
            }

            Console.WriteLine($"{logMessage}");
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
                Console.WriteLine($"Total time: {1000.0 * (float)totalCpuElapsed / cpuFrequency:F4}ms (CPU freq {cpuFrequency})\n");
            }

            foreach (var anchor in GlobalProfiler.Anchors)
            {
                if (anchor.TSCElapsed != 0)
                {
                    PrintTimeElapsed(totalCpuElapsed, anchor);
                }
            }
        }
    }
}
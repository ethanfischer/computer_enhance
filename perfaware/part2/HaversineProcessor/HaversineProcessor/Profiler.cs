using JsonGenerator;
using static ProfilerService;
using static JsonGenerator.TimerService;


public struct ProfileAnchor
{
    public ulong TSCElapsed;
    public ulong TSCElapsedChildren;
    public ulong HitCount;
    public string Label;
}

public struct Profiler
{
    public readonly ProfileAnchor[] Anchors = new ProfileAnchor[4096];
    public ulong StartTSC;
    public ulong EndTSC;

    public Profiler()
    {
        StartTSC = 0;
        EndTSC = 0;
    }
}

public static class ProfilerService
{
    public static Profiler GlobalProfiler = new();
    public static uint ProfilerParent;
    private static uint _counter = 0;

    public static string NameConcat(string a, string b) => a + b;

    public static ProfileBlock TimeBlock(string label) => new(label, ++_counter);
// #define TimeFunction TimeBlock(__func__)

    public static void PrintTimeElapsed(ulong totalTSCElapsed, ProfileAnchor anchor)
    {
        var elapsed = anchor.TSCElapsed - anchor.TSCElapsedChildren;
        var percent = 100.0 * ((float)elapsed / totalTSCElapsed);
        Console.WriteLine($"{anchor.Label}[{anchor.HitCount}]: {elapsed} ({percent:F2}%)");
        if (anchor.TSCElapsedChildren > 0)
        {
            var percentWithChildren = 100.0 * ((float)anchor.TSCElapsed / totalTSCElapsed);
            Console.WriteLine($"w/children: {percentWithChildren:F2}\n");
        }
    }

    public static void BeginProfile()
    {
        GlobalProfiler.StartTSC = ReadCPUTimer();
    }

    public static void EndAndPrintProfile()
    {
        GlobalProfiler.EndTSC = ReadCPUTimer();
        var cpuFrequency = EstimateCPUTimerFreq();

        var totalCpuElapsed = GlobalProfiler.EndTSC - GlobalProfiler.StartTSC;

        if (cpuFrequency > 0)
        {
            Console.WriteLine($"Total time: {1000.0 * (float)totalCpuElapsed / cpuFrequency:F4}ms (CPU freq {cpuFrequency})");
        }

        for (var anchorIndex = 0; anchorIndex < GlobalProfiler.Anchors.Length; ++anchorIndex)
        {
            var anchor = GlobalProfiler.Anchors[anchorIndex];
            if (anchor.TSCElapsed != 0)
            {
                PrintTimeElapsed(totalCpuElapsed, anchor);
            }
        }
    }
}

public struct ProfileBlock : IDisposable
{
    public string Label;
    public ulong StartTSC;
    public uint ParentIndex;
    public uint AnchorIndex;

    public ProfileBlock(string label, uint anchorIndex)
    {
        ParentIndex = ProfilerParent;

        AnchorIndex = anchorIndex;
        Label = label;

        ProfilerParent = AnchorIndex;
        StartTSC = ReadCPUTimer();
    }

    public void Dispose()
    {
        var elapsed = ReadCPUTimer() - StartTSC;
        ProfilerParent = ParentIndex;

        var parent = GlobalProfiler.Anchors[ParentIndex];
        var anchor = GlobalProfiler.Anchors[AnchorIndex];

        parent.TSCElapsedChildren += elapsed;
        anchor.TSCElapsed += elapsed;
        anchor.HitCount++;

        /* NOTE(casey): This write happens every time solely because there is no
           straightforward way in C++ to have the same ease-of-use. In a better programming
           language, it would be simple to have the anchor points gathered and labeled at compile
           time, and this repetative write would be eliminated. */
        anchor.Label = Label;

        GlobalProfiler.Anchors[ParentIndex] = parent;
        GlobalProfiler.Anchors[AnchorIndex] = anchor;
    }
};
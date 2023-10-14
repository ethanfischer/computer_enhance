using Newtonsoft.Json;
using SMXGo.Scripts.Other;
using Spectre.Console;
namespace HaversineProcessor;

public static class RepititionTester
{
    static long _minorFaults;
    static long _majorFaults;
    public static void Test(Func<bool, ProfilerReport> test, bool shouldAllocateMemory)
    {
        var minTotalCpuElapsed = ulong.MaxValue;
        var maxTotalCpuElapsed = ulong.MinValue;
        ulong totalTotalCpuElapsed = 0;

        double minBandwidth = 0f;
        double maxBandwidth = 0f;
        double totalBandwidths = 0;

        double minSeconds = 0f;
        double maxSeconds = 0f;
        double totalSeconds = 0f;

        var iterations = 0;

        var maxWaitingTime = TimeSpan.FromSeconds(10);
        var timeSinceLastNewMin = new TimeSpan();
        var previousTime = DateTime.UtcNow;
        var originalTop = Console.CursorTop;

        var table = new Table();

        // Assume some initial values
        string min;
        string max;
        string avg;

        AnsiConsole.Live(table)
            .AutoClear(false) // Prevents the table from being cleared after the live display ends
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
            {
                table.AddColumn("Stat");
                table.AddColumn("Value");

                while (timeSinceLastNewMin < maxWaitingTime)
                {
                    var report = test.Invoke(shouldAllocateMemory);
                    var seconds = (double)report.TotalCpuElapsed / report.CpuFrequency;
                    const float gb = 1024f * 1024f * 1024f;
                    var bandwidth = report.BytesProcessed / (gb * seconds);

                    if (report.TotalCpuElapsed < minTotalCpuElapsed)
                    {
                        minTotalCpuElapsed = report.TotalCpuElapsed;
                        timeSinceLastNewMin = TimeSpan.Zero;
                        minBandwidth = bandwidth;
                        minSeconds = seconds;
                    }
                    if (report.TotalCpuElapsed > maxTotalCpuElapsed)
                    {
                        maxTotalCpuElapsed = report.TotalCpuElapsed;
                        maxBandwidth = bandwidth;
                        maxSeconds = seconds;
                    }

                    totalTotalCpuElapsed += report.TotalCpuElapsed;
                    totalBandwidths += bandwidth;
                    totalSeconds += seconds;
                    iterations++;

                    min = $"{minTotalCpuElapsed} ({minSeconds * 1000:F2}ms) {minBandwidth:F2}gb/s";
                    max = $"{maxTotalCpuElapsed} ({maxSeconds * 1000:F2}ms) {maxBandwidth:F2}gb/s";
                    avg = $"{totalTotalCpuElapsed / (ulong)iterations} ({totalSeconds / iterations * 1000:F2}ms) {totalBandwidths / iterations:F2}gb/s";

                    timeSinceLastNewMin += DateTime.UtcNow - previousTime;
                    previousTime = DateTime.UtcNow;


                    table.Rows.Clear();
                    table.AddRow("Min", min);
                    table.AddRow("Max", max);
                    table.AddRow("Avg", avg);

                    ctx.Refresh();

                    // Some delay to see the update, replace with your loop logic
                    Thread.Sleep(1000);
                }
            });

        var rUsage = MacPerformanceMetrics.GetRUsage();
        _majorFaults = rUsage.ru_majflt - _majorFaults;
        _minorFaults = rUsage.ru_minflt - _minorFaults;
        Console.WriteLine($"Major Page Faults: {_majorFaults}");
        Console.WriteLine($"Minor Page Faults: {_minorFaults}");
    }

    static void LogReport(ProfilerReport report)
    {
        // Console.WriteLine($"CpuFrequency: {report.CpuFrequency}");
        Console.WriteLine($"TotalCpuElapsed: {report.TotalCpuElapsed}");
        // Console.WriteLine($"ProfileAnchors: {GetSerializedAnchors(report)}");
        // Console.WriteLine($"PairCount: {report.PairCount}");
        // Console.WriteLine($"HaversineSum: {report.HaversineSum}");
    }

    static string GetSerializedAnchors(ProfilerReport report)
    {
        var profileAnchors = report.Anchors.Where(x => x.HitCount > 0);
        var serializedAnchors = JsonConvert.SerializeObject(profileAnchors);
        return serializedAnchors;
    }
}
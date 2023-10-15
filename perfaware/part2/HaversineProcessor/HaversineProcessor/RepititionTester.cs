using Newtonsoft.Json;
using SMXGo.Scripts.Other;
using Spectre.Console;
namespace HaversineProcessor;

public static class RepititionTester
{
    public static void Test(Func<bool, ProfilerReport> test, bool shouldAllocateMemory)
    {
        long _memPageFaults;
        var maxWaitingTime = TimeSpan.FromSeconds(60);
        var timeSinceLastNewMin = new TimeSpan();
        var previousTime = DateTime.UtcNow;

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

                var repetitionReport = new RepetitionReport();

                while (timeSinceLastNewMin < maxWaitingTime)
                {
                    var faultsBeforeTest = GetMemFaults();
                    var testReport = test.Invoke(shouldAllocateMemory);
                    _memPageFaults = GetMemFaults() - faultsBeforeTest;
                    var seconds = (double)testReport.TotalCpuElapsed / testReport.CpuFrequency;
                    const float gb = 1024f * 1024f * 1024f;
                    var bandwidth = testReport.BytesProcessed / (gb * seconds);


                    if (testReport.TotalCpuElapsed < repetitionReport.MinTotalCpuElapsed)
                    {
                        repetitionReport.MinTotalCpuElapsed = testReport.TotalCpuElapsed;
                        timeSinceLastNewMin = TimeSpan.Zero;
                        repetitionReport.MinBandwidth = bandwidth;
                        repetitionReport.MinSeconds = seconds;
                        repetitionReport.MinFaults = _memPageFaults;
                    }
                    if (testReport.TotalCpuElapsed > repetitionReport.MaxTotalCpuElapsed)
                    {
                        repetitionReport.MaxTotalCpuElapsed = testReport.TotalCpuElapsed;
                        repetitionReport.MaxBandwidth = bandwidth;
                        repetitionReport.MaxSeconds = seconds;
                        repetitionReport.MaxFaults = _memPageFaults;
                    }

                    repetitionReport.TotalTotalCpuElapsed += testReport.TotalCpuElapsed;
                    repetitionReport.TotalBandwidths += bandwidth;
                    repetitionReport.TotalSeconds += seconds;
                    repetitionReport.TotalFaults += _memPageFaults;
                    repetitionReport.Iterations++;

                    min = $"{repetitionReport.MinTotalCpuElapsed} ({repetitionReport.MinSeconds * 1000:F2}ms) {repetitionReport.MinBandwidth:F2}gb/s PF: {repetitionReport.MinFaults} ({testReport.BytesProcessed / repetitionReport.MinFaults / 1024:F2}k/fault)";
                    max = $"{repetitionReport.MaxTotalCpuElapsed} ({repetitionReport.MaxSeconds * 1000:F2}ms) {repetitionReport.MaxBandwidth:F2}gb/s PF: {repetitionReport.MaxFaults} ({testReport.BytesProcessed / repetitionReport.MaxFaults / 1024:F2}k/fault)";
                    var avgFaults = repetitionReport.TotalFaults / repetitionReport.Iterations;
                    avg = $"{repetitionReport.TotalTotalCpuElapsed / (ulong)repetitionReport.Iterations} ({repetitionReport.TotalSeconds / repetitionReport.Iterations * 1000:F2}ms) {repetitionReport.TotalBandwidths / repetitionReport.Iterations:F2}gb/s PF: {avgFaults:F2} ({testReport.BytesProcessed / avgFaults / 1024:F2}k/fault)";

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
    }

    static long GetMemFaults()
    {
        var rUsage = MacPerformanceMetrics.GetRUsage();
        return rUsage.ru_minflt + rUsage.ru_majflt;
    }

    struct RepetitionReport
    {
        public RepetitionReport() { }

        public ulong MinTotalCpuElapsed = ulong.MaxValue;
        public ulong MaxTotalCpuElapsed = ulong.MinValue;
        public ulong TotalTotalCpuElapsed = 0;
        public double MinBandwidth = 0f;
        public double MaxBandwidth = 0f;
        public double TotalBandwidths = 0;
        public double MinSeconds = 0f;
        public double MaxSeconds = 0f;
        public double TotalSeconds = 0f;
        public double MaxFaults = 0f;
        public double MinFaults = 0f;
        public double TotalFaults = 0f;
        public int Iterations = 0;
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
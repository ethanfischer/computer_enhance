using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json;
using SMXGo.Scripts.Other;
namespace HaversineProcessor;

public static class RepititionTester
{
    public static void Test(Func<bool, ProfilerReport> test, bool shouldAllocateMemory)
    {
        var minTotalCpuElapsed = ulong.MaxValue;
        var lastNewMinTime = DateTime.UtcNow;
        var maxWaitingTime = TimeSpan.FromSeconds(10);
        var timeSinceLastNewMin = new TimeSpan();
        var previousTime = DateTime.UtcNow;

        while (timeSinceLastNewMin < maxWaitingTime)
        {
            var report = test.Invoke(shouldAllocateMemory);
            var seconds = (double)report.TotalCpuElapsed / report.CpuFrequency;
            const float gb = 1024f * 1024f * 1024f;
            var bandwidth = report.BytesProcessed / (gb * seconds);
            
            if (report.TotalCpuElapsed < minTotalCpuElapsed)
            {
                minTotalCpuElapsed = report.TotalCpuElapsed;
                WriteLineAndClear($"Min: {minTotalCpuElapsed} ({seconds * 1000:F2}ms) {bandwidth:F2}gb/s");
                lastNewMinTime = DateTime.UtcNow;
                timeSinceLastNewMin = TimeSpan.Zero;
            }

            timeSinceLastNewMin += DateTime.UtcNow - previousTime;
            previousTime = DateTime.UtcNow;
        }
        Console.WriteLine();
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
    
    static void WriteLineAndClear(string line)
    {
        int currentLine = Console.CursorTop;
        Console.SetCursorPosition(0, currentLine);
        Console.Write(new string(' ', Console.WindowWidth));  // Clear the line
        Console.SetCursorPosition(0, currentLine);
        Console.Write(line);
    }
}
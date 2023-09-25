using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json;
using SMXGo.Scripts.Other;
namespace HaversineProcessor;

public static class RepititionTester
{
    public static void Test(Func<ProfilerReport> test)
    {
        var minTotalCpuElapsed = ulong.MaxValue;
        var lastNewMinTime = DateTime.UtcNow;
        var maxWaitingTime = TimeSpan.FromMinutes(30);
        var timeSinceLastNewMin = new TimeSpan();
        var previousTime = DateTime.UtcNow;

        while (timeSinceLastNewMin < maxWaitingTime)
        {
            var report = test.Invoke();
            if(report.TotalCpuElapsed < minTotalCpuElapsed)
            {
                minTotalCpuElapsed = report.TotalCpuElapsed;
                var gb = 1024f * 1024f * 1024f;
                var seconds = (double)report.TotalCpuElapsed / report.CpuFrequency;
                var bestBandwidth = report.BytesProcessed / (gb * seconds);
                Console.WriteLine($"Min {minTotalCpuElapsed} ({seconds*1000:F2}ms) {bestBandwidth:F2}gb/s");
                lastNewMinTime = DateTime.UtcNow;
                timeSinceLastNewMin = TimeSpan.Zero;
            }

            timeSinceLastNewMin += DateTime.UtcNow - previousTime;
            previousTime = DateTime.UtcNow;
        }
        
        Console.WriteLine($"Min {minTotalCpuElapsed}");
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
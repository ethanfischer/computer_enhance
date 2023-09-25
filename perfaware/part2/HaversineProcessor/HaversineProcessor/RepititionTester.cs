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
        var maxWaitingTime = TimeSpan.FromMilliseconds(10_000);
        var timeSinceLastNewMin = new TimeSpan();

        while (timeSinceLastNewMin < maxWaitingTime)
        {
            var report = test.Invoke();
            if(report.TotalCpuElapsed < minTotalCpuElapsed)
            {
                minTotalCpuElapsed = report.TotalCpuElapsed;
                Console.WriteLine($"New Min {minTotalCpuElapsed}");
                lastNewMinTime = DateTime.UtcNow;
            }
            
            timeSinceLastNewMin += DateTime.UtcNow - lastNewMinTime;
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
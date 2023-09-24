using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SMXGo.Scripts.Other;
namespace HaversineProcessor;

public static class RepititionTester
{
    public static void Test(Func<ProfilerReport> test)
    {

        for (var i = 0; i < 10; i++)
        {
            var report = test.Invoke();
            Console.WriteLine($"Report {i+1}");
            Console.WriteLine($"-----------------------------------");
            LogReport(report);
            Console.WriteLine($"-----------------------------------");
            Console.WriteLine($"");
        }

    }

    static void LogReport(ProfilerReport report)
    {
        Console.WriteLine($"CpuFrequency: {report.CpuFrequency}");
        Console.WriteLine($"TotalCpuElapsed: {report.TotalCpuElapsed}");
        Console.WriteLine($"ProfileAnchors: {GetSerializedAnchors(report)}");
        Console.WriteLine($"PairCount: {report.PairCount}");
        Console.WriteLine($"HaversineSum: {report.HaversineSum}");
    }
    
    static string GetSerializedAnchors(ProfilerReport report)
    {
        var profileAnchors = report.Anchors.Where(x => x.HitCount > 0);
        var serializedAnchors = JsonConvert.SerializeObject(profileAnchors);
        return serializedAnchors;
    }
}
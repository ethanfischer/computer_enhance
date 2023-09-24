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
            ProfilerReport report = test.Invoke();
            Console.WriteLine($"Report {i+1}");
            Console.WriteLine($"-----------------------------------");
            LogReport(report);
        }

    }

    private static void LogReport(ProfilerReport report)
    {
        Console.WriteLine($"{report.CpuFrequency}");
        Console.WriteLine($"{report.TotalCpuElapsed}");
        var profileAnchors = report.Anchors.Where(x => x.HitCount > 0);
        Console.WriteLine($"{JsonConvert.SerializeObject(profileAnchors)}");
    }
}
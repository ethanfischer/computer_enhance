using Newtonsoft.Json;
using SMXGo.Scripts.Other;
namespace HaversineProcessor;

public static class RepititionTester
{
    public static void Test(Func<ProfilerReport> test)
    {
        const int reportsCount = 1;
        var reports = new ProfilerReport[reportsCount];
        var avgTotalCpuElapsed = 0.0;
        var maxTotalCpuElapsed = 0.0;

        for (var i = 0; i < reportsCount; i++)
        {
            reports[i] = test.Invoke();
            Console.WriteLine($"Report {i + 1}");
            Console.WriteLine($"-----------------------------------");
            LogReport(reports[i]);
            Console.WriteLine($"");
            avgTotalCpuElapsed = reports.Select(x => x.TotalCpuElapsed).Average(Convert.ToDouble);
            maxTotalCpuElapsed = reports.Select(x => x.TotalCpuElapsed).Max(Convert.ToDouble);
            Console.WriteLine($"Average: {avgTotalCpuElapsed}");
            Console.WriteLine($"Max: {maxTotalCpuElapsed}");
            Console.WriteLine($"-----------------------------------");
            Console.WriteLine($"");
            avgTotalCpuElapsed = reports.Select(x => x.TotalCpuElapsed).Average(Convert.ToDouble);
            Console.WriteLine($"");
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
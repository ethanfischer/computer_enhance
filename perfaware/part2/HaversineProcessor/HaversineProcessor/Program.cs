// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using JsonGenerator;

internal class Program
{
    // public static void Main(string[] args)
    // {
    //     var t_begin = CpuTimerService.ReadCpuTimer();
    //     Console.WriteLine($"Time starting");
    //     var t_startup = CpuTimerService.ReadCpuTimer();
    //     var jsonBytes = File.ReadAllBytes("C:/Users/ethan.fischer/GitProjects/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/data.json");
    //     var t_read = CpuTimerService.ReadCpuTimer();
    //     var pairs = JsonParser.Deserialize(jsonBytes);
    //     var t_parse = CpuTimerService.ReadCpuTimer();
    //     var answer = File.ReadAllText("C:/Users/ethan.fischer/GitProjects/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/answer.txt");
    //     var pairCount = pairs.Count;
    //     var sum = 0.00d;
    //     var t_miscSetup = CpuTimerService.ReadCpuTimer();
    //     foreach (var pair in pairs)
    //     {
    //         sum += Haversine.ReferenceHaversine(pair.X0, pair.Y0, pair.X1, pair.Y1);
    //     }
    //
    //     var t_sum = CpuTimerService.ReadCpuTimer();
    //
    //     Console.WriteLine($"Pair count: {pairCount}");
    //     Console.WriteLine($"Haversine sum: {sum / pairCount}");
    //     Console.WriteLine("");
    //     
    //     var t_end = CpuTimerService.ReadCpuTimer();
    //     var cpuFrequency = (float)CpuTimerService.EstimateCpuFrequency(1000);
    //     var t = (float)t_end - t_begin;
    //     Console.WriteLine($"Total time: {t / cpuFrequency * 1000}ms (CPU Frequency {cpuFrequency})");
    //     Console.WriteLine($"Total: {t} ({t / t * 100:F2}%)");
    //     var s = t_startup - t_begin;
    //     Console.WriteLine($"Startup: {s} ({s / t * 100:F2}%)");
    //     var r = t_read - t_startup;
    //     Console.WriteLine($"Read: {r} ({r / t * 100:F2}%)");
    //     var p = t_parse - t_read;
    //     Console.WriteLine($"Parse: {p} ({p / t * 100:F2}%)");
    //     var su = t_sum - t_miscSetup;
    //     Console.WriteLine($"Sum: {su} ({su / t * 100:F2}%)");
    //     var m = t_end - t_sum;
    //     Console.WriteLine($"MiscOutput: {m} ({m / t * 100:F2}%)");
    // }
}
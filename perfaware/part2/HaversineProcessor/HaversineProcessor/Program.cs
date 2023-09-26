// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using HaversineProcessor;
using JsonGenerator;
using SMXGo.Scripts.Other;
using static SMXGo.Scripts.Other.SMXProfiler;

internal class Program
{
    public static void Main(string[] args)
    {
        RepititionTester.Test(RunHaversine);
    }
    
    static ProfilerReport RunHaversine()
    {
        BeginProfile();

        byte[] jsonBytes;
        using (TimeBlock("Read Json from Disk"))
        {
            jsonBytes = File.ReadAllBytes("/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/data.json");
        }

        var pairs = JsonParser.Deserialize(jsonBytes);

        var answer = File.ReadAllText("/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/answer.txt");
        var pairCount = pairs.Count;

        var sum = 0d;
        using (TimeBlock("Pair summation"))
        {
            foreach (var pair in pairs)
            {
                sum += Haversine.ReferenceHaversine(pair.X0, pair.Y0, pair.X1, pair.Y1);
            }
        }

        var additionalResult = new ProfilerReport
        {
            PairCount = pairCount,
            HaversineSum = sum / pairCount
        };
        var result = EndAndPrintProfile();
        result.PairCount = additionalResult.PairCount;
        result.HaversineSum = additionalResult.HaversineSum;
        // Console.WriteLine($"{result.PairCount}");
        // Console.WriteLine($"{result.HaversineSum}");
        return result;
    }

    private static void Recursive(int depth = 0)
    {
        using var _ = TimeBlock("Recursive");
        
        if (depth > 10)
        {
            return;
        }

        Thread.Sleep(1000);

        Recursive(depth + 1);
    }
}
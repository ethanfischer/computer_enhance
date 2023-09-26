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
        RunHaversine();
    }

    static void RunHaversine()
    {
        byte[] jsonBytes =
        {
        };
        using (TimeBlock("Read Json from Disk"))
        {
            RepititionTester.Test(() =>
            {
                BeginProfile();
                jsonBytes = File.ReadAllBytes("/home/ethan/repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/data.json");
                return EndAndGetReport(jsonBytes.Length);
            });

        }

        var pairs = JsonParser.Deserialize(jsonBytes);

        var answer = File.ReadAllText("/home/ethan/repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/answer.txt");
        var pairCount = pairs.Count;

        var sum = 0d;
        using (TimeBlock("Pair summation"))
        {
            foreach (var pair in pairs)
            {
                sum += Haversine.ReferenceHaversine(pair.X0, pair.Y0, pair.X1, pair.Y1);
            }
        }
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
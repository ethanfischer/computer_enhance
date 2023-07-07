// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using JsonGenerator;
using static ProfilerService;

internal class Program
{
    public static void Main(string[] args)
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

        Console.WriteLine($"Pair count: {pairCount}");
        Console.WriteLine($"Haversine sum: {sum / pairCount}");
        Console.WriteLine("");
        EndAndPrintProfile();
    }

    // private static double GetSum(List<Pair> pairs)
    // {
    //     TimeFunction();
    //     var sum = 0d;
    //     foreach (var pair in pairs)
    //     {
    //         sum += Haversine.ReferenceHaversine(pair.X0, pair.Y0, pair.X1, pair.Y1);
    //     }
    //
    //     return sum;
    // }
}
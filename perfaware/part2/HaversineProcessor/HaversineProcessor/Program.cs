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

        Recursive();

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

    private static void Recursive(int depth = 0)
    {
        if (depth > 3) { return; }
        
        Thread.Sleep(3000);

        using (TimeBlock($"Recursive {depth}"))
        {
            Recursive(depth + 1);
        }
    }
}
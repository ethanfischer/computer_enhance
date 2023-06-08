// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using JsonGenerator;

internal class Program
{
    public static void Main(string[] args)
    {
        var jsonBytes = File.ReadAllBytes("/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/data.json");
        var pairs = JsonParser.Deserialize(jsonBytes);
        var answer = File.ReadAllText("/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/answer.txt");
        var count = pairs.Count;
        var sum = 0.00d;
        foreach(var pair in pairs)
        {
            sum += Haversine.ReferenceHaversine(pair.X0, pair.Y0, pair.X1, pair.Y1);
        }
        
        Console.WriteLine($"Answer {sum/count}");
        Console.WriteLine($"Expected answer {answer}");
    }
}
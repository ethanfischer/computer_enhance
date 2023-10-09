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
        const int size = 588_124_160;
        var jsonBytes = new byte[size];
        var shouldAllocateMemory = false;
        while (true)
        {
            Console.WriteLine($"{(shouldAllocateMemory ? "Allocating" : "Not Allocating")}");
            RepititionTester.Test(shouldAllocate =>
            {
                jsonBytes = AllocateJsonBytes(size, shouldAllocate, jsonBytes);
                BeginProfile();
                //jsonBytes = File.ReadAllBytes("C:/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/data.json");
                using (var fs = new FileStream("C:/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/data.json", FileMode.Open, FileAccess.Read))
                {
                    fs.Read(jsonBytes, 0, jsonBytes.Length);
                }

                return EndAndGetReport(jsonBytes.Length);
            }, shouldAllocateMemory);
            shouldAllocateMemory = !shouldAllocateMemory;
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
    }
    
    static byte[] AllocateJsonBytes(int size, bool shouldMalloc, byte[] existingBytes)
    {
        return shouldMalloc ? new byte[size] : existingBytes;
    }
}
// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using JsonGenerator;

internal class Program
{
    private static bool _shouldUseClusters;

    public static void Main(string[] args)
    {
        Console.WriteLine("Should I use clusters? (Y/n)");
        _shouldUseClusters = Console.ReadLine() == "";
        Console.WriteLine("How many pairs should I generate?");
        var count = int.Parse(Console.ReadLine());
        Console.WriteLine("Seed");
        var seed = int.Parse(Console.ReadLine());
        var random = new Random(seed);
        var clusters = CreateClusters();

        var pairs = new Pair[count];
        double sum = 0.0f;
        for (var i = 0; i < count; i++)
        {
            Pair p;
            if (_shouldUseClusters)
            {
                var cluster1 = clusters[random.Next(0, 64)];
                var cluster2 = clusters[random.Next(0, 64)];
                p = new Pair(cluster1.Item1, cluster2.Item1, cluster1.Item2, cluster2.Item2);
            }
            else
            {
                p = new Pair(random.Next(-180, 180), random.Next(-180, 180), random.Next(-90, 90), random.Next(-90, 90));
            }
            sum += Haversine.ReferenceHaversine(p.X0, p.Y0, p.X1, p.Y1);
            pairs[i] = p;
        }

        var json = JsonSerializer.Serialize(pairs);
        File.WriteAllText("/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/data.json", json);

        var expectedAvg = sum / count;
        Console.WriteLine($"Expected avg: {expectedAvg}");
        File.WriteAllText("/Users/ethanfischer/Repos/computer_enhance/perfaware/part2/JsonGeneration/JsonGenerator/JsonGenerator/answer.txt", expectedAvg.ToString());
        Main(default);
    }

    private static (double, double)[] CreateClusters()
    {
        var result = new (double, double)[64];
        for (var i = 0; i < 64; i++)
        {
            result[i] = new ValueTuple<double, double>(
                -180.0f + i * 4.21875,
                -90.0f + i * 2.109375
                );
        }

        return result;
    }

    struct Pair
    {
        public double X0 { get; }
        public double X1 { get; }
        public double Y0 { get; }
        public double Y1 { get; }

        public Pair(double x0, double x1, double y0, double y1)
        {
            X0 = x0;
            X1 = x1;
            Y0 = y0;
            Y1 = y1;
        }
    }
}

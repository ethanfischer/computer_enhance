using System.Reflection;
using static SMXGo.Scripts.Other.SMXProfiler;

namespace JsonGenerator;

public static class JsonParser
{
    private static bool _isLookingForClosingBrace;

    public static List<Pair> Deserialize(byte[] jsonBytes)
    {
        // Console.WriteLine($"jsonBytes.Length: {jsonBytes.Length}");
        using var _ = TimeBlock("Deserialize");
        var partials = PopulatePartials(jsonBytes);

        using (TimeBlock("PopulateMembers"))
        {
            foreach (var p in partials)
            {
                PopulateMembers(p);
            }
        }

        var result = partials.Select(x => x.Value).ToList();
        return result;
    }

    private static void PopulateMembers(PartiallyDeserialized<Pair> partial)
    {
        var json = partial.Json;
        var isLookingForClosingQuote = false;
        var quotedString = string.Empty;
        PropertyInfo property = null;
        var didEncounterColon = false;
        var propertyValue = string.Empty;

        foreach (var ch in json)
        {
            if (property != null)
            {
                if (didEncounterColon)
                {
                    if (ch == '}' || ch == ',')
                    {
                        didEncounterColon = false;
                        var t = property.PropertyType;
                        if (t == typeof(double))
                        {
                            var value = double.Parse(propertyValue);
                            property.SetValue(partial.Value, value);
                            propertyValue = string.Empty;
                            property = null;
                            quotedString = string.Empty;
                            continue;
                        }
                    }
                    else
                    {
                        propertyValue += ch;
                        continue;
                    }
                }
                else if (ch == ':')
                {
                    didEncounterColon = true;
                    continue;
                }
            }
            else
            {
                if (isLookingForClosingQuote)
                {
                    if (ch == '"')
                    {
                        isLookingForClosingQuote = false;

                        property = typeof(Pair).GetProperty(quotedString);
                        if (property == null)
                        {
                            throw new Exception($"property {quotedString} not found on Pair");
                        }
                    }
                    else
                    {
                        quotedString += ch;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    isLookingForClosingQuote = true;
                }
            }
        }
    }

    private static List<PartiallyDeserialized<Pair>> PopulatePartials(byte[] jsonBytes)
    {
        var partials = new List<PartiallyDeserialized<Pair>>();
        var partialJson = string.Empty;
        foreach (var b in jsonBytes)
        {
            var ch = (char)b;
            // Console.WriteLine($"{ch}, {b.Binary()}");

            if (_isLookingForClosingBrace)
            {
                if (ch == '}')
                {
                    partialJson += ch;
                    _isLookingForClosingBrace = false;
                    partials.Add(new PartiallyDeserialized<Pair>(partialJson, new Pair()));
                    partialJson = string.Empty;
                }
                else
                {
                    partialJson += ch;
                }

                continue;
            }

            if (ch == '{')
            {
                _isLookingForClosingBrace = true;
            }
        }

        if (_isLookingForClosingBrace)
        {
            throw new Exception("missing closing brace in json");
        }

        return partials;
    }

    private static string Binary(this byte b)
    {
        return Convert.ToString(b, 2).PadLeft(8, '0');
    }

    private class PartiallyDeserialized<T>
    {
        public PartiallyDeserialized(string json, T value)
        {
            Json = json;
            Value = value;
        }

        public string Json { get; set; }
        public T Value { get; set; }
    }
}
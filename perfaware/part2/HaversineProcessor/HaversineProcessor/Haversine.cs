namespace JsonGenerator;

public class Haversine
{
    static double Square(double a)
    {
        var result = a * a;
        return result;
    }

    static double RadiansFromDegrees(double degrees)
    {
        var result = 0.01745329251994329577f * degrees;
        return result;
    }

    // NOTE(casey): EarthRadius is generally expected to be 6372.8
    public static double ReferenceHaversine(double x0, double y0, double x1, double y1, double earthRadius = 6372.8)
    {
        /* NOTE(casey): This is not meant to be a "good" way to calculate the Haversine distance.
           Instead, it attempts to follow, as closely as possible, the formula used in the real-world
           question on which these homework exercises are loosely based.
        */

        var lat1 = y0;
        var lat2 = y1;

        var dLat = RadiansFromDegrees(lat2 - lat1);
        var dLon = RadiansFromDegrees(x1 - x0);
        lat1 = RadiansFromDegrees(lat1);
        lat2 = RadiansFromDegrees(lat2);

        var a = Square(Math.Sin(dLat / 2.0f)) + Math.Cos(lat1) * Math.Cos(lat2) * Square(Math.Sin(dLon / 2));
        var c = 2.0 * Math.Asin(Math.Sqrt(a));

        var result = earthRadius * c;

        return result;
    }
}

namespace Aplication.Rendering;

/// <summary>Rozmieszczenie pojedynczego wagonu w przestrzeni mapy: środek, kąt i długość pola.</summary>
public readonly record struct WagonPlacement(double CenterX, double CenterY, double Angle, double Length);

/// <summary>
/// Geometria tras w przestrzeni mapy: budowa łamanej (From → waypoints → To) oraz podział na
/// równe pola-wagony rozmieszczone wzdłuż łuku. Ta sama łamana służy rysowaniu i hit-testingowi
/// (renderowanie-mapy.md §4.2, §6.3) — jedno źródło prawdy o kształcie trasy.
/// </summary>
public static class RouteGeometry
{
    /// <summary>
    /// Łamana trasy: środek miasta początkowego, punkty pośrednie, środek miasta końcowego.
    /// </summary>
    public static IReadOnlyList<MapPoint> BuildPolyline(Route route, IReadOnlyDictionary<string, City> cities)
    {
        var points = new List<MapPoint>(route.Waypoints.Count + 2)
        {
            cities[route.CityFromId].Position
        };
        points.AddRange(route.Waypoints);
        points.Add(cities[route.CityToId].Position);
        return points;
    }

    /// <summary>
    /// Dzieli łamaną na <paramref name="wagonCount"/> równych pól wzdłuż łuku, z marginesem
    /// <paramref name="endMargin"/> (jednostki mapy) od końców, by wagony nie wchodziły pod miasta.
    /// Każdy wagon dostaje środek i kąt zgodny z lokalnym kierunkiem łamanej.
    /// </summary>
    public static IReadOnlyList<WagonPlacement> BuildWagons(
        IReadOnlyList<MapPoint> polyline, int wagonCount, double endMargin)
    {
        var total = TotalLength(polyline);
        var usable = Math.Max(0, total - 2 * endMargin);
        var slot = usable / wagonCount;

        var result = new List<WagonPlacement>(wagonCount);
        for (var i = 0; i < wagonCount; i++)
        {
            var centerDist = endMargin + (i + 0.5) * slot;
            var (point, angle) = SampleAt(polyline, centerDist);
            result.Add(new WagonPlacement(point.X, point.Y, angle, slot));
        }

        return result;
    }

    /// <summary>Najmniejsza odległość punktu od łamanej (dla hit-testingu trasy).</summary>
    public static double DistanceToPolyline(MapPoint p, IReadOnlyList<MapPoint> polyline)
    {
        var min = double.MaxValue;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            min = Math.Min(min, DistanceToSegment(p, polyline[i], polyline[i + 1]));
        }

        return min;
    }

    public static double TotalLength(IReadOnlyList<MapPoint> polyline)
    {
        var sum = 0.0;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            sum += Distance(polyline[i], polyline[i + 1]);
        }

        return sum;
    }

    /// <summary>Zwraca punkt i kąt stycznej na łamanej w zadanej odległości łukowej od początku.</summary>
    private static (MapPoint Point, double Angle) SampleAt(IReadOnlyList<MapPoint> polyline, double distance)
    {
        var remaining = distance;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var a = polyline[i];
            var b = polyline[i + 1];
            var segLen = Distance(a, b);
            if (segLen <= 0)
            {
                continue;
            }

            if (remaining <= segLen)
            {
                var t = remaining / segLen;
                var point = new MapPoint(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
                var angle = Math.Atan2(b.Y - a.Y, b.X - a.X);
                return (point, angle);
            }

            remaining -= segLen;
        }

        // Poza końcem — zwróć ostatni segment.
        var last = polyline[^1];
        var prev = polyline[^2];
        return (last, Math.Atan2(last.Y - prev.Y, last.X - prev.X));
    }

    private static double Distance(MapPoint a, MapPoint b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double DistanceToSegment(MapPoint p, MapPoint a, MapPoint b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lenSq = dx * dx + dy * dy;
        if (lenSq <= 0)
        {
            return Distance(p, a);
        }

        var t = Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq, 0, 1);
        var projX = a.X + t * dx;
        var projY = a.Y + t * dy;
        var ddx = p.X - projX;
        var ddy = p.Y - projY;
        return Math.Sqrt(ddx * ddx + ddy * ddy);
    }
}

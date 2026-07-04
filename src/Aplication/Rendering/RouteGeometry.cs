namespace Aplication.Rendering;

/// <summary>
/// Geometria tras w przestrzeni mapy. Kształt trasy to gotowa łamana z danych
/// (<see cref="Aplication.Models.Route.Points"/>) — tu liczymy tylko odległość punktu od łamanej
/// na potrzeby hit-testingu.
/// </summary>
public static class RouteGeometry
{
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

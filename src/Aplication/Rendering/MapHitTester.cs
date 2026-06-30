namespace Aplication.Rendering;

/// <summary>Rodzaj trafionego elementu mapy.</summary>
public enum MapHitKind { None, City, Route }

/// <summary>Wynik hit-testingu: rodzaj elementu i jego identyfikator (pusty dla <see cref="MapHitKind.None"/>).</summary>
public readonly record struct MapHit(MapHitKind Kind, string Id)
{
    public static readonly MapHit None = new(MapHitKind.None, string.Empty);
}

/// <summary>
/// Geometryczny hit-testing w przestrzeni mapy (renderowanie-mapy.md §6). Punkt dotyku jest
/// przeliczany odwrotną transformacją do przestrzeni mapy, gdzie geometria jest stała. Próg
/// trafienia skaluje się odwrotnie do zoomu, by cel pozostał klikalny przy każdym powiększeniu.
/// Miasta mają priorytet nad trasami; przy kilku kandydatach wygrywa najbliższy.
/// </summary>
public sealed class MapHitTester
{
    private readonly MapData _map;
    private readonly IReadOnlyList<(string Id, IReadOnlyList<MapPoint> Polyline)> _routePolylines;

    public MapHitTester(MapData map)
    {
        _map = map;
        var cities = map.Cities.ToDictionary(c => c.Id);
        _routePolylines = map.Routes
            .Select(r => (r.Id, RouteGeometry.BuildPolyline(r, cities)))
            .ToList();
    }

    public MapHit HitTest(PointF screen, MapViewport viewport)
    {
        var tap = viewport.ScreenToMap(screen.X, screen.Y);

        if (TryHitCity(tap, viewport.Scale, out var cityId))
        {
            return new MapHit(MapHitKind.City, cityId);
        }

        if (TryHitRoute(tap, viewport.Scale, out var routeId))
        {
            return new MapHit(MapHitKind.Route, routeId);
        }

        return MapHit.None;
    }

    private bool TryHitCity(MapPoint tap, double scale, out string cityId)
    {
        var hitRadius = Math.Max(MapMetrics.CityRadius, MapMetrics.MinTouchTarget / scale);
        var bestDist = double.MaxValue;
        cityId = string.Empty;

        foreach (var city in _map.Cities)
        {
            var dx = tap.X - city.X;
            var dy = tap.Y - city.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist <= hitRadius && dist < bestDist)
            {
                bestDist = dist;
                cityId = city.Id;
            }
        }

        return cityId.Length > 0;
    }

    private bool TryHitRoute(MapPoint tap, double scale, out string routeId)
    {
        var hitWidth = Math.Max(MapMetrics.WagonHalfWidth, MapMetrics.MinTouchTarget / 2 / scale);
        var bestDist = double.MaxValue;
        routeId = string.Empty;

        foreach (var (id, polyline) in _routePolylines)
        {
            var dist = RouteGeometry.DistanceToPolyline(tap, polyline);
            if (dist <= hitWidth && dist < bestDist)
            {
                bestDist = dist;
                routeId = id;
            }
        }

        return routeId.Length > 0;
    }
}

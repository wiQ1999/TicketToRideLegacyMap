namespace Aplication.Rendering;

/// <summary>Rodzaj trafionego elementu mapy.</summary>
public enum MapHitKind { None, City, Route }

/// <summary>Wynik hit-testingu: rodzaj elementu i jego identyfikator (pusty dla <see cref="MapHitKind.None"/>).</summary>
public readonly record struct MapHit(MapHitKind Kind, string Id)
{
    public static readonly MapHit None = new(MapHitKind.None, string.Empty);
}

/// <summary>
/// Geometryczny hit-testing w przestrzeni mapy. Punkt dotyku jest przeliczany odwrotną
/// transformacją do przestrzeni mapy, gdzie geometria jest stała i niezależna od zoomu.
/// Bez marginesu tolerancji — klikalny obszar odpowiada dokładnie temu, co narysowane.
/// Miasta mają priorytet nad trasami; przy kilku kandydatach wygrywa najbliższy.
/// </summary>
public sealed class MapHitTester(MapData map)
{
    public MapHit HitTest(PointF screen, MapViewport viewport)
    {
        var tap = viewport.ScreenToMap(screen.X, screen.Y);

        if (TryHitCity(tap, out var cityId))
        {
            return new MapHit(MapHitKind.City, cityId);
        }

        if (TryHitRoute(tap, out var routeId))
        {
            return new MapHit(MapHitKind.Route, routeId);
        }

        return MapHit.None;
    }

    private bool TryHitCity(MapPoint tap, out string cityId)
    {
        var bestDist = double.MaxValue;
        cityId = string.Empty;

        foreach (var city in map.Cities)
        {
            var dx = tap.X - city.X;
            var dy = tap.Y - city.Y;
            var dist = Math.Sqrt((dx * dx) + (dy * dy));
            if (dist <= MapMetrics.CityRadius && dist < bestDist)
            {
                bestDist = dist;
                cityId = city.Id;
            }
        }

        return cityId.Length > 0;
    }

    private bool TryHitRoute(MapPoint tap, out string routeId)
    {
        var bestDist = double.MaxValue;
        routeId = string.Empty;

        foreach (var route in map.Routes)
        {
            foreach (var wagon in route.Wagons)
            {
                if (!wagon.Contains(tap))
                {
                    continue;
                }

                var center = wagon.Center;
                var dx = tap.X - center.X;
                var dy = tap.Y - center.Y;
                var dist = Math.Sqrt((dx * dx) + (dy * dy));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    routeId = route.Id;
                }
            }
        }

        return routeId.Length > 0;
    }
}

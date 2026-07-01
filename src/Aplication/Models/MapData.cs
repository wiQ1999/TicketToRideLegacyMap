namespace Aplication.Models;

/// <summary>
/// Kompletny, niemutowalny układ planszy: zakres przestrzeni mapy oraz listy miast i tras.
/// Reprezentuje jeden, stały stan planszy.
/// </summary>
public sealed class MapData(MapSize canvasSize, IReadOnlyList<City> cities, IReadOnlyList<Route> routes)
{
    public MapSize CanvasSize { get; } = canvasSize;

    public IReadOnlyList<City> Cities { get; } = cities;

    public IReadOnlyList<Route> Routes { get; } = routes;
}

/// <summary>Zakres przestrzeni mapy (szerokość × wysokość) w jednostkach logicznych.</summary>
public readonly record struct MapSize(double Width, double Height);

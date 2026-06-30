namespace Aplication.Models;

/// <summary>
/// Kompletny, niemutowalny układ planszy: zakres przestrzeni mapy oraz listy miast i tras.
/// Reprezentuje jeden, stały stan planszy.
/// </summary>
public sealed class MapData(MapSize canvasSize, IReadOnlyList<City> cities, IReadOnlyList<Route> routes)
{
    /// <summary>Zakres przestrzeni mapy (do dopasowania widoku „z lotu ptaka").</summary>
    public MapSize CanvasSize { get; } = canvasSize;

    /// <summary>Miasta planszy.</summary>
    public IReadOnlyList<City> Cities { get; } = cities;

    /// <summary>Trasy (połączenia) planszy.</summary>
    public IReadOnlyList<Route> Routes { get; } = routes;
}

/// <summary>Zakres przestrzeni mapy (szerokość × wysokość) w jednostkach logicznych.</summary>
public readonly record struct MapSize(double Width, double Height);

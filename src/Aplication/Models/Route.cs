namespace Aplication.Models;

/// <summary>
/// Trasa (połączenie) między dwoma miastami — niemutowalny element mapy bazowej.
/// Liczba wagonów oraz kolor są wartościami stałymi wbudowanymi w mapę. Stan
/// zaznaczenia/wykonania trzymany jest osobno w serwisie stanu.
/// </summary>
public sealed class Route(
    string id,
    string cityFromId,
    string cityToId,
    int wagonCount,
    RouteColor color,
    IReadOnlyList<MapPoint>? waypoints = null)
{
    /// <summary>Unikatowy identyfikator trasy (klucz stanu trasy).</summary>
    public string Id { get; } = id;

    /// <summary>Identyfikator miasta początkowego.</summary>
    public string CityFromId { get; } = cityFromId;

    /// <summary>Identyfikator miasta końcowego.</summary>
    public string CityToId { get; } = cityToId;

    /// <summary>Stała liczba wagonów — długość trasy i liczba prostokątów do narysowania.</summary>
    public int WagonCount { get; } = wagonCount;

    /// <summary>Kolor wymaganych kart; <see cref="RouteColor.Gray"/> = trasa neutralna.</summary>
    public RouteColor Color { get; } = color;

    /// <summary>
    /// Opcjonalne punkty pośrednie (łamana zamiast prostego odcinka) — dla tras łukowatych
    /// lub równoległych. Pusta lista oznacza prosty odcinek między miastami.
    /// </summary>
    public IReadOnlyList<MapPoint> Waypoints { get; } = waypoints ?? Array.Empty<MapPoint>();
}

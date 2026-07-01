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
    RouteColor color,
    IReadOnlyList<MapPoint> points)
{
    public string Id { get; } = id;

    public string CityFromId { get; } = cityFromId;

    public string CityToId { get; } = cityToId;

    public RouteColor Color { get; } = color;

    public IReadOnlyList<MapPoint> Points { get; } = points;

    public int WagonCount => Points.Count - 1;
}

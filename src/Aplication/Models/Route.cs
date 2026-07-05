namespace Aplication.Models;

/// <summary>
/// Trasa (połączenie) między dwoma miastami — niemutowalny element mapy bazowej. Kształt trasy to
/// lista niezależnych prostokątów wagoników; liczba wagonów oraz kolor są wartościami stałymi
/// wbudowanymi w mapę. Stan zaznaczenia/wykonania trzymany jest osobno w serwisie stanu.
/// </summary>
public sealed class Route(
    string id,
    string cityFromId,
    string cityToId,
    IReadOnlyList<WagonRectangle> wagons)
{
    public string Id { get; } = id;

    public string CityFromId { get; } = cityFromId;

    public string CityToId { get; } = cityToId;

    public IReadOnlyList<WagonRectangle> Wagons { get; } = wagons;

    public int WagonCount => Wagons.Count;
}

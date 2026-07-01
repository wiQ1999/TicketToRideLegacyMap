namespace Aplication.Models;

/// <summary>
/// Miasto na planszy — niemutowalny element mapy bazowej.
/// Stan oznaczenia miasta trzymany jest osobno w serwisie stanu, nie tutaj.
/// </summary>
public sealed class City(string id, string name, double x, double y)
{
    public string Id { get; } = id;

    public string Name { get; } = name;

    public double X { get; } = x;

    public double Y { get; } = y;

    public MapPoint Position => new(X, Y);
}

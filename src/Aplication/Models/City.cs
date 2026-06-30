namespace Aplication.Models;

/// <summary>
/// Miasto na planszy — niemutowalny element mapy bazowej.
/// Stan oznaczenia miasta trzymany jest osobno w serwisie stanu, nie tutaj.
/// </summary>
public sealed class City(string id, string name, double x, double y)
{
    /// <summary>Unikatowy identyfikator miasta (klucz dla tras i stanu oznaczeń).</summary>
    public string Id { get; } = id;

    /// <summary>Nazwa wyświetlana.</summary>
    public string Name { get; } = name;

    /// <summary>Pozycja X środka miasta w przestrzeni mapy.</summary>
    public double X { get; } = x;

    /// <summary>Pozycja Y środka miasta w przestrzeni mapy.</summary>
    public double Y { get; } = y;

    /// <summary>Pozycja środka miasta jako <see cref="MapPoint"/>.</summary>
    public MapPoint Position => new(X, Y);
}

namespace Aplication.Models;

/// <summary>
/// Kolor trasy zgodny z oryginalną grą — określa, jakich kart wagonów wymaga trasa.
/// Jest cechą mapy bazowej, niezależną od stanu zaznaczenia/wykonania.
/// <see cref="Gray"/> oznacza trasę neutralną (dowolny kolor kart).
/// </summary>
public enum RouteColor
{
    /// <summary>Trasa neutralna — dowolny kolor kart wagonów.</summary>
    Gray,
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Pink,
    White,
    Black
}

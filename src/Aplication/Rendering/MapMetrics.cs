namespace Aplication.Rendering;

/// <summary>
/// Stałe geometryczne renderowania i hit-testingu, w jednostkach przestrzeni mapy (o ile nie
/// zaznaczono inaczej). Wspólne dla rysowania i trafień, dobrane pod przestrzeń podkładu 168×283.
/// Przy podmianie danych na większą planszę wystarczy je tu przeskalować.
/// </summary>
public static class MapMetrics
{
    /// <summary>Promień okręgu miasta (średnica = 2×). Steruje wielkością kropki miasta.</summary>
    public const double CityRadius = 14.0;
}

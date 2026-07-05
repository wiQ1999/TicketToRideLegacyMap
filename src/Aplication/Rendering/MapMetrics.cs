namespace Aplication.Rendering;

/// <summary>
/// Stałe geometryczne renderowania i hit-testingu, w jednostkach przestrzeni mapy (o ile nie
/// zaznaczono inaczej). Wspólne dla rysowania i trafień, dobrane pod przestrzeń podkładu 1448×1086.
/// Przy podmianie danych na inną planszę wystarczy je tu przeskalować.
/// </summary>
public static class MapMetrics
{
    /// <summary>Promień okręgu miasta (średnica = 2×). Steruje wielkością kropki miasta.</summary>
    public const double CityRadius = 16.0;
}

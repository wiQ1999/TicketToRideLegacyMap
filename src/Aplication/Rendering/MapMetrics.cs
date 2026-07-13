namespace Aplication.Rendering;

/// <summary>
/// Stałe geometryczne renderowania i hit-testingu, w jednostkach przestrzeni mapy (o ile nie
/// zaznaczono inaczej). Wspólne dla rysowania i trafień, dobrane pod przestrzeń podkładu 1448×1086.
/// Przy podmianie danych na inną planszę wystarczy je tu przeskalować.
/// </summary>
public static class MapMetrics
{
    /// <summary>Promień okręgu miasta (średnica = 2×). Steruje wielkością kropki miasta.</summary>
    public const double CityRadius = 20.0;

    /// <summary>Grubość białego obramowania oznaczonego miasta (w jednostkach mapy).</summary>
    public const double CityMarkBorderWidth = 2.0;

    /// <summary>Skala ikony gwiazdy względem średnicy okręgu miasta (0–1).</summary>
    public const double CityStarScale = 0.6;
}

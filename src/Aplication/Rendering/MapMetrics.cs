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

    /// <summary>Grubość obrysu wagonika zaznaczonej/wykonanej trasy (w jednostkach mapy).</summary>
    public const double WagonBorderWidth = 2.0;

    /// <summary>Grubość ukośnej kreski wzoru wypełnienia wagonika (w jednostkach mapy).</summary>
    public const double WagonStripeWidth = 5.0;

    /// <summary>Odstęp (okres) między kolejnymi ukośnymi kreskami wzoru wagonika (w jednostkach mapy).</summary>
    public const double WagonStripeSpacing = 13.0;

    /// <summary>Kąt ukośnych kresek wzoru wagonika względem jego dłuższego boku (w stopniach).</summary>
    public const double WagonStripeAngleDegrees = 45.0;

    /// <summary>Skala ikony kłódki (trasa wykonana) względem krótszego boku wagonika (0–1).</summary>
    public const double WagonLockScale = 0.8;
}

namespace Aplication.Rendering;

/// <summary>
/// Stałe geometryczne renderowania i hit-testingu, w jednostkach przestrzeni mapy (o ile nie
/// zaznaczono inaczej). Wspólne dla rysowania i trafień, dobrane pod przestrzeń podkładu 168×283.
/// Przy podmianie danych na większą planszę wystarczy je tu przeskalować.
/// </summary>
public static class MapMetrics
{
    /// <summary>Promień okręgu miasta (średnica = 2×). Steruje wielkością kropki miasta.</summary>
    public const double CityRadius = 13.0;

    /// <summary>Grubość pierścienia oznaczenia miasta (obrys).</summary>
    public const double CityMarkRingWidth = 3.0;

    /// <summary>Połowa grubości linii trasy (i bazowy próg trafienia trasy).</summary>
    public const double WagonHalfWidth = 8.0;

    /// <summary>
    /// Minimalny komfortowy rozmiar celu dotyku w pikselach ekranu (DIP). Próg trafienia
    /// dzielony przez skalę utrzymuje stały rozmiar celu na ekranie niezależnie od zoomu.
    /// </summary>
    public const double MinTouchTarget = 28.0;

    /// <summary>Poniżej tej skali etykiety miast są ukrywane (odszumienie widoku ogólnego).</summary>
    public const double LabelMinScale = 2.2;
}

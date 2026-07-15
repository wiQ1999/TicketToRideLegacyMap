namespace Aplication.Rendering;

/// <summary>
/// Statyczne kolory renderowania planszy, oderwane od koloru gracza (ten liczy
/// <see cref="RouteColorPalette"/>). Trzymane w jednym miejscu, by renderer nie zawierał literałów kolorów.
/// </summary>
public static class MapPalette
{
    /// <summary>Białe obramowanie oznaczonego miasta (koło pod znacznikiem).</summary>
    public static readonly Color CityMarkBorder = Colors.White;

    /// <summary>Cień odcinający znacznik oznaczonego miasta od tła planszy.</summary>
    public static readonly Color CityMarkShadow = Color.FromArgb("#66000000");

    /// <summary>Ukośny wzór wypełnienia wagonika zaznaczonej/wykonanej trasy.</summary>
    public static readonly Color WagonStripe = Color.FromRgba(255, 255, 255, 90);

    /// <summary>Znaczniki nakładki roboczej w trybie deweloperskim (miasta, trasy, wskazany punkt).</summary>
    public static readonly Color DeveloperMark = Color.FromArgb("#EC407A");

    /// <summary>Neutralne tło planszy rysowane, gdy brak podkładu graficznego.</summary>
    public static readonly Color BoardFallback = Color.FromArgb("#E9DBB8");

    /// <summary>Pergaminowe tło obszaru poza planszą (widoczne po oddaleniu/przesunięciu).</summary>
    public static readonly Color OutsideBase = Color.FromArgb("#E7D6AE");

    /// <summary>Kropki delikatnej siatki na tle poza planszą.</summary>
    public static readonly Color OutsideDot = Color.FromRgba(107, 74, 43, 38);
}

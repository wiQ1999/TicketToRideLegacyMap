namespace Aplication.Rendering;

/// <summary>Odwzorowanie palety koloru wagonów gracza (<see cref="WagonColor"/>) na kolor ekranowy.</summary>
public static class RouteColorPalette
{
    public static Color ToColor(WagonColor color) => color switch
    {
        WagonColor.Czarny => Color.FromArgb("#212121"),
        WagonColor.Czerwony => Color.FromArgb("#C62828"),
        WagonColor.Niebieski => Color.FromArgb("#1565C0"),
        WagonColor.Zielony => Color.FromArgb("#2E7D32"),
        WagonColor.Zolty => Color.FromArgb("#F9A825"),
        _ => Color.FromArgb("#212121")
    };
}

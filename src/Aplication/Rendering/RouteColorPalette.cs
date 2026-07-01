namespace Aplication.Rendering;

/// <summary>
/// Centralne mapowanie <see cref="RouteColor"/> (kolor kart trasy — cecha mapy) na kolor RGB
/// oraz kolor gracza dla tras wykonanych. Trzymane w jednym miejscu, by łatwo dostroić paletę
/// na etapie UI.
/// </summary>
public static class RouteColorPalette
{
    public static readonly Color Player = Color.FromArgb("#2E7D32");

    public static Color ForRoute(RouteColor color) => color switch
    {
        RouteColor.Red => Color.FromArgb("#D32F2F"),
        RouteColor.Orange => Color.FromArgb("#F57C00"),
        RouteColor.Yellow => Color.FromArgb("#FBC02D"),
        RouteColor.Green => Color.FromArgb("#388E3C"),
        RouteColor.Blue => Color.FromArgb("#1976D2"),
        RouteColor.Pink => Color.FromArgb("#E91E63"),
        RouteColor.White => Color.FromArgb("#FAFAFA"),
        RouteColor.Black => Color.FromArgb("#212121"),
        _ => Color.FromArgb("#9E9E9E") // Gray = trasa neutralna
    };
}

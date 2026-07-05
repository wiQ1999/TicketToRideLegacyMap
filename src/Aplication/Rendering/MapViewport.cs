namespace Aplication.Rendering;

/// <summary>
/// Stan transformacji widoku mapy: skala i przesunięcie odwzorowujące przestrzeń mapy
/// (logiczną, stałą) na przestrzeń ekranu (DIP wewnątrz kontrolki). Jeden obiekt wspólny
/// dla rysowania i hit-testingu — co widać, to jest klikalne.
/// </summary>
public sealed class MapViewport(double mapWidth, double mapHeight)
{
    /// <summary>Maksymalne powiększenie względem widoku „z lotu ptaka".</summary>
    public const double MaxZoom = 8.0;

    public double MapWidth { get; } = mapWidth;
    public double MapHeight { get; } = mapHeight;

    public double ViewWidth { get; private set; }
    public double ViewHeight { get; private set; }

    /// <summary>Skala dopasowania całej planszy do kadru — dolny limit zoomu.</summary>
    public double FitScale { get; private set; } = 1.0;

    public double Scale { get; private set; } = 1.0;
    public double OffsetX { get; private set; }
    public double OffsetY { get; private set; }

    public bool HasView => ViewWidth > 0 && ViewHeight > 0;

    /// <summary>
    /// Ustawia rozmiar kadru i (re)kalibruje widok „z lotu ptaka": fit-to-screen + wyśrodkowanie.
    /// Wywoływane przy starcie i każdej zmianie rozmiaru.
    /// </summary>
    public void ResetToFit(double viewWidth, double viewHeight)
    {
        ViewWidth = viewWidth;
        ViewHeight = viewHeight;
        if (!HasView)
        {
            return;
        }

        FitScale = Math.Min(viewWidth / MapWidth, viewHeight / MapHeight);
        Scale = FitScale;
        OffsetX = (viewWidth - (MapWidth * Scale)) / 2.0;
        OffsetY = (viewHeight - (MapHeight * Scale)) / 2.0;
    }

    public PointF MapToScreen(double mapX, double mapY) =>
        new((float)((mapX * Scale) + OffsetX), (float)((mapY * Scale) + OffsetY));

    public PointF MapToScreen(MapPoint p) => MapToScreen(p.X, p.Y);

    public MapPoint ScreenToMap(double screenX, double screenY) =>
        new((screenX - OffsetX) / Scale, (screenY - OffsetY) / Scale);

    /// <summary>Przesuwa widok o wektor w pikselach ekranu, z ograniczeniem (clamp).</summary>
    public void PanBy(double dxScreen, double dyScreen)
    {
        OffsetX += dxScreen;
        OffsetY += dyScreen;
        ClampOffset();
    }

    /// <summary>
    /// Skaluje widok do <paramref name="targetScale"/> wokół punktu ekranu
    /// <paramref name="pivotScreen"/> tak, by punkt mapy pod gestem pozostał nieruchomy.
    /// </summary>
    public void ZoomTo(double targetScale, PointF pivotScreen)
    {
        var clamped = Math.Clamp(targetScale, FitScale, FitScale * MaxZoom);
        var pivotMap = ScreenToMap(pivotScreen.X, pivotScreen.Y);
        Scale = clamped;
        // Dobierz Offset tak, by pivotMap mapował się z powrotem na pivotScreen.
        OffsetX = pivotScreen.X - (pivotMap.X * Scale);
        OffsetY = pivotScreen.Y - (pivotMap.Y * Scale);
        ClampOffset();
    }

    /// <summary>
    /// Programowo przybliża i centruje widok na punkcie mapy <paramref name="target"/> (np. wynik
    /// wyszukiwania miasta) — bez udziału gestu. Skala to <paramref name="zoomFactor"/>-krotność
    /// widoku „z lotu ptaka", ograniczona do dozwolonego zakresu zoomu.
    /// </summary>
    public void CenterOn(MapPoint target, double zoomFactor = 4.0)
    {
        if (!HasView)
        {
            return;
        }

        Scale = Math.Clamp(FitScale * zoomFactor, FitScale, FitScale * MaxZoom);
        OffsetX = (ViewWidth / 2.0) - (target.X * Scale);
        OffsetY = (ViewHeight / 2.0) - (target.Y * Scale);
        ClampOffset();
    }

    /// <summary>
    /// Utrzymuje co najmniej fragment planszy w kadrze. Gdy plansza jest mniejsza od kadru
    /// w danej osi (po wpasowaniu), pozostaje wyśrodkowana w tej osi.
    /// </summary>
    private void ClampOffset()
    {
        OffsetX = ClampAxis(OffsetX, MapWidth * Scale, ViewWidth);
        OffsetY = ClampAxis(OffsetY, MapHeight * Scale, ViewHeight);
    }

    private static double ClampAxis(double offset, double contentSize, double viewSize)
    {
        if (contentSize <= viewSize)
        {
            return (viewSize - contentSize) / 2.0;
        }

        // Nie pozwól odsłonić pustego marginesu poza planszą.
        var min = viewSize - contentSize;
        return Math.Clamp(offset, min, 0);
    }
}

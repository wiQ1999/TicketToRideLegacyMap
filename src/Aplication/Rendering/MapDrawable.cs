namespace Aplication.Rendering;

/// <summary>
/// Rysuje całą planszę w jednym przebiegu: tło (opcjonalny podkład) → trasy → miasta. Nie przechowuje
/// stanu interakcji — w trybie mapy przy każdym rysowaniu odpytuje <see cref="IMapInteractionState"/>
/// o stan trasy i oznaczenie miasta. W trybie deweloperskim (<paramref name="state"/> = <c>null</c>)
/// rysuje jedynie podkład oraz nakładkę roboczą. Wszystkie współrzędne liczy <see cref="MapViewport"/>,
/// więc rysowanie i hit-testing pozostają spójne.
/// </summary>
public sealed class MapDrawable(MapData map, MapViewport viewport, IMapInteractionState? state) : IDrawable
{
    private static readonly Color CityMarkBorderColor = Colors.White;
    private static readonly Color CityMarkShadowColor = Color.FromArgb("#66000000");
    private static readonly Color DeveloperMarkerColor = Color.FromArgb("#1565C0");
    private static readonly Color DeveloperPendingCityColor = Color.FromArgb("#EF6C00");
    private static readonly Color DeveloperPendingWagonColor = Color.FromArgb("#00897B");

    public Microsoft.Maui.Graphics.IImage? Background { get; set; }

    /// <summary>Biała ikona gwiazdy rysowana na środku oznaczonego miasta.</summary>
    public Microsoft.Maui.Graphics.IImage? CityStar { get; set; }

    /// <summary>Robocze miasta (tryb deweloperski) rysowane jako znaczniki nad podkładem.</summary>
    public IReadOnlyList<MapPoint>? DeveloperMarkers { get; set; }

    /// <summary>Wskazany na mapie, jeszcze niezatwierdzony punkt (tryb deweloperski).</summary>
    public MapPoint? DeveloperPendingPoint { get; set; }

    /// <summary>
    /// Czy <see cref="DeveloperPendingPoint"/> to róg wagonika trasy, a nie pozycja miasta — decyduje
    /// o kolorze znacznika, tak by oba tryby (miasta/trasy) były wizualnie rozróżnialne.
    /// </summary>
    public bool DeveloperPendingPointIsWagonCorner { get; set; }

    /// <summary>Wagoniki roboczej trasy (dodawanej lub edytowanej) rysowane jako nakładka (tryb deweloperski).</summary>
    public IReadOnlyList<WagonRectangle>? DeveloperWagons { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawBackground(canvas);

        if (state is { } interactionState)
        {
            foreach (var route in map.Routes)
            {
                DrawRoute(canvas, route, interactionState);
            }

            foreach (var city in map.Cities)
            {
                DrawCity(canvas, city, interactionState);
            }

            return;
        }

        DrawDeveloperOverlay(canvas);
    }

    private void DrawDeveloperOverlay(ICanvas canvas)
    {
        if (DeveloperWagons is { } wagons)
        {
            canvas.StrokeColor = DeveloperMarkerColor;
            canvas.StrokeSize = 2f;
            foreach (var wagon in wagons)
            {
                var corners = wagon.Corners.Select(viewport.MapToScreen).ToArray();
                var path = new PathF();
                path.MoveTo(corners[0]);
                for (var i = 1; i < corners.Length; i++)
                {
                    path.LineTo(corners[i]);
                }

                path.Close();
                canvas.DrawPath(path);
            }
        }

        if (DeveloperMarkers is { } markers)
        {
            canvas.FillColor = DeveloperMarkerColor;
            foreach (var marker in markers)
            {
                var center = viewport.MapToScreen(marker);
                canvas.FillCircle(center.X, center.Y, (float)(MapMetrics.CityRadius * viewport.Scale));
            }
        }

        if (DeveloperPendingPoint is { } pending)
        {
            var center = viewport.MapToScreen(pending);
            var radius = (float)(MapMetrics.CityRadius * viewport.Scale);
            canvas.StrokeColor = DeveloperPendingPointIsWagonCorner ? DeveloperPendingWagonColor : DeveloperPendingCityColor;
            canvas.StrokeSize = 3f;
            canvas.DrawCircle(center.X, center.Y, radius);
            canvas.DrawLine(center.X - radius, center.Y, center.X + radius, center.Y);
            canvas.DrawLine(center.X, center.Y - radius, center.X, center.Y + radius);
        }
    }

    private void DrawBackground(ICanvas canvas)
    {
        var origin = viewport.MapToScreen(0, 0);
        var width = (float)(map.CanvasSize.Width * viewport.Scale);
        var height = (float)(map.CanvasSize.Height * viewport.Scale);

        if (Background is not null)
        {
            canvas.DrawImage(Background, origin.X, origin.Y, width, height);
        }
        else
        {
            canvas.FillColor = Color.FromArgb("#E9DBB8"); // neutralne tło planszy
            canvas.FillRectangle(origin.X, origin.Y, width, height);
        }
    }

    private void DrawRoute(ICanvas canvas, Route route, IMapInteractionState state)
    {
        var routeState = state.GetRouteState(route.Id);

        // Domyślnie trasa jest przezroczysta — kolor trasy widać z podkładu (tła).
        if (routeState == RouteState.None)
        {
            return;
        }

        canvas.StrokeLineJoin = LineJoin.Miter;
        canvas.StrokeLineCap = LineCap.Butt;

        foreach (var wagon in route.Wagons)
        {
            DrawWagon(canvas, wagon, routeState, state.WagonColor);
        }
    }

    private void DrawWagon(ICanvas canvas, WagonRectangle wagon, RouteState routeState, WagonColor wagonColor)
    {
        var corners = wagon.Corners.Select(viewport.MapToScreen).ToArray();
        var path = new PathF();
        path.MoveTo(corners[0]);
        for (var i = 1; i < corners.Length; i++)
        {
            path.LineTo(corners[i]);
        }

        path.Close();

        var playerColor = RouteColorPalette.ToColor(wagonColor);

        if (routeState == RouteState.Done)
        {
            // Wykonana: pełne wypełnienie kolorem wagonów gracza.
            canvas.FillColor = playerColor;
            canvas.FillPath(path);
        }
        else
        {
            // Zaznaczona: sam obrys kolorem wagonów gracza, wnętrze przezroczyste — inny kanał
            // wizualny niż wypełnienie, więc rozróżnialne także przy zaburzeniach widzenia barw.
            canvas.StrokeColor = playerColor;
            canvas.StrokeSize = 3f;
            canvas.DrawPath(path);
        }
    }

    private void DrawCity(ICanvas canvas, City city, IMapInteractionState state)
    {
        // Bez oznaczenia miasto jest przezroczyste (nic nie rysujemy).
        if (!state.IsCityMarked(city.Id))
        {
            return;
        }

        // Oznaczone: powiększony okrąg w kolorze akcentu z białym obramowaniem, cieniem i gwiazdą.
        var center = viewport.MapToScreen(city.X, city.Y);
        var radius = (float)(MapMetrics.CityRadius * viewport.Scale);
        var borderWidth = (float)(MapMetrics.CityMarkBorderWidth * viewport.Scale);

        // Białe koło pod spodem tworzy obramowanie; cień odcina znacznik od tła planszy.
        canvas.SaveState();
        canvas.SetShadow(new SizeF(0f, borderWidth), borderWidth * 1.5f, CityMarkShadowColor);
        canvas.FillColor = CityMarkBorderColor;
        canvas.FillCircle(center.X, center.Y, radius);
        canvas.RestoreState();

        canvas.FillColor = RouteColorPalette.ToColor(state.WagonColor);
        canvas.FillCircle(center.X, center.Y, radius - borderWidth);

        if (CityStar is { } star)
        {
            var starSize = radius * 2f * (float)MapMetrics.CityStarScale;
            canvas.DrawImage(star, center.X - starSize / 2f, center.Y - starSize / 2f, starSize, starSize);
        }
    }
}

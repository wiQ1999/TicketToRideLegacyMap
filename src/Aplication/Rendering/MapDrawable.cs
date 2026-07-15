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
    public Microsoft.Maui.Graphics.IImage? Background { get; set; }

    /// <summary>Biała ikona gwiazdy rysowana na środku oznaczonego miasta.</summary>
    public Microsoft.Maui.Graphics.IImage? CityStar { get; set; }

    /// <summary>Biała ikona kłódki rysowana na wagonikach trasy wykonanej.</summary>
    public Microsoft.Maui.Graphics.IImage? WagonLock { get; set; }

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
        DrawOutsidePattern(canvas, dirtyRect);
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
        // Trasa robocza: sam prostokąt wagonika (obrys), widoczny po zaznaczeniu drugiego rogu.
        if (DeveloperWagons is { } wagons)
        {
            canvas.StrokeColor = MapPalette.DeveloperMark;
            canvas.StrokeSize = (float)(MapMetrics.WagonBorderWidth * viewport.Scale);
            canvas.StrokeLineJoin = LineJoin.Round;
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

        // Robocze miasta: sam obrys okręgu, bez wypełnienia.
        if (DeveloperMarkers is { } markers)
        {
            canvas.StrokeColor = MapPalette.DeveloperMark;
            canvas.StrokeSize = (float)(MapMetrics.CityMarkBorderWidth * viewport.Scale);
            var radius = (float)(MapMetrics.CityRadius * viewport.Scale);
            foreach (var marker in markers)
            {
                var center = viewport.MapToScreen(marker);
                canvas.DrawCircle(center.X, center.Y, radius);
            }
        }

        // Wskazany, niezatwierdzony punkt miasta: sam obrys okręgu. Róg wagonika trasy nie ma osobnego
        // znacznika — trasę widać dopiero jako prostokąt po zaznaczeniu drugiego rogu.
        if (DeveloperPendingPoint is { } pending && !DeveloperPendingPointIsWagonCorner)
        {
            var center = viewport.MapToScreen(pending);
            canvas.StrokeColor = MapPalette.DeveloperMark;
            canvas.StrokeSize = (float)(MapMetrics.CityMarkBorderWidth * viewport.Scale);
            canvas.DrawCircle(center.X, center.Y, (float)(MapMetrics.CityRadius * viewport.Scale));
        }
    }

    private static void DrawOutsidePattern(ICanvas canvas, RectF dirtyRect)
    {
        // Pełne tło pod całą kontrolką; podkład planszy (nieprzezroczysty) rysowany jest na wierzchu,
        // więc siatka kropek zostaje widoczna tylko w marginesie poza planszą.
        canvas.FillColor = MapPalette.OutsideBase;
        canvas.FillRectangle(dirtyRect);

        var spacing = MapMetrics.OutsideDotSpacing;
        canvas.FillColor = MapPalette.OutsideDot;

        // Siatka zakotwiczona do (0,0) ekranu, o stałym rozmiarze (niezależnym od zoomu). Co drugi rząd
        // przesunięty o pół odstępu — delikatny, „utkany" wzór.
        var row = 0;
        for (var y = dirtyRect.Top; y <= dirtyRect.Bottom; y += spacing, row++)
        {
            var offset = (row % 2 == 0) ? 0f : spacing / 2f;
            for (var x = dirtyRect.Left + offset; x <= dirtyRect.Right; x += spacing)
            {
                canvas.FillCircle(x, y, MapMetrics.OutsideDotRadius);
            }
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
            canvas.FillColor = MapPalette.BoardFallback;
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

        // Zaznaczona i wykonana: pełne wypełnienie kolorem gracza z ukośnym wzorem i ciemnym obrysem.
        // Wykonaną dodatkowo znaczy kłódka — inny kanał wizualny niż sam kolor, więc rozróżnialna
        // także przy zaburzeniach widzenia barw.
        canvas.FillColor = playerColor;
        canvas.FillPath(path);

        canvas.SaveState();
        canvas.ClipPath(path);
        DrawWagonStripes(canvas, corners);
        canvas.RestoreState();

        canvas.StrokeColor = Darken(playerColor, (float)MapMetrics.WagonBorderDarkenFactor);
        canvas.StrokeSize = (float)(MapMetrics.WagonBorderWidth * viewport.Scale);
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(path);

        if (routeState == RouteState.Done && WagonLock is { } lockIcon)
        {
            var shorter = Math.Min(Distance(corners[0], corners[1]), Distance(corners[1], corners[2]));
            var lockSize = (float)(shorter * MapMetrics.WagonLockScale);
            var center = new PointF(corners.Average(p => p.X), corners.Average(p => p.Y));
            canvas.DrawImage(lockIcon, center.X - lockSize / 2f, center.Y - lockSize / 2f, lockSize, lockSize);
        }
    }

    private void DrawWagonStripes(ICanvas canvas, PointF[] corners)
    {
        var step = (float)(MapMetrics.WagonStripeSpacing * viewport.Scale);
        if (step < 0.5f)
        {
            return;
        }

        // Kierunek kresek liczony względem samego wagonika: oś dłuższego boku (róg 1 → róg 2) plus
        // zadany kąt, więc wzór jest pod 45° do boków wagonika niezależnie od jego obrotu na planszy.
        var axis = Math.Atan2(corners[2].Y - corners[1].Y, corners[2].X - corners[1].X);
        var angle = axis + (MapMetrics.WagonStripeAngleDegrees * Math.PI / 180.0);
        var dir = new PointF((float)Math.Cos(angle), (float)Math.Sin(angle));
        var normal = new PointF(-dir.Y, dir.X);

        // Rzuty rogów na kierunek kresek (dir) i prostopadły (normal) wyznaczają zakres wzoru.
        float pMin = float.MaxValue, pMax = float.MinValue, qMin = float.MaxValue, qMax = float.MinValue;
        foreach (var corner in corners)
        {
            var p = (corner.X * normal.X) + (corner.Y * normal.Y);
            var q = (corner.X * dir.X) + (corner.Y * dir.Y);
            pMin = Math.Min(pMin, p);
            pMax = Math.Max(pMax, p);
            qMin = Math.Min(qMin, q);
            qMax = Math.Max(qMax, q);
        }

        canvas.StrokeColor = MapPalette.WagonStripe;
        canvas.StrokeSize = (float)(MapMetrics.WagonStripeWidth * viewport.Scale);
        canvas.StrokeLineCap = LineCap.Butt;

        // Równoległe kreski (kierunek dir), przesuwane co „step" wzdłuż prostopadłej; ClipPath tnie je
        // do kształtu wagonika.
        for (var t = pMin; t <= pMax; t += step)
        {
            var start = new PointF((t * normal.X) + (qMin * dir.X), (t * normal.Y) + (qMin * dir.Y));
            var end = new PointF((t * normal.X) + (qMax * dir.X), (t * normal.Y) + (qMax * dir.Y));
            canvas.DrawLine(start, end);
        }
    }

    private static float Distance(PointF a, PointF b) => (float)Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));

    private static Color Darken(Color color, float factor) =>
        Color.FromRgba(color.Red * factor, color.Green * factor, color.Blue * factor, color.Alpha);

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
        canvas.SetShadow(
            new SizeF(0f, borderWidth * (float)MapMetrics.CityMarkShadowOffsetScale),
            borderWidth * (float)MapMetrics.CityMarkShadowBlurScale,
            MapPalette.CityMarkShadow);
        canvas.FillColor = MapPalette.CityMarkBorder;
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

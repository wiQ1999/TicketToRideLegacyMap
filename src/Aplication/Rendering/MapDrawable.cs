namespace Aplication.Rendering;

/// <summary>
/// Rysuje całą planszę w jednym przebiegu (renderowanie-mapy.md §2.2): tło (opcjonalny podkład) →
/// wagony tras → miasta → oznaczenia stanów → etykiety. Nie przechowuje stanu interakcji —
/// odpytuje <see cref="IMapInteractionState"/> przy każdym rysowaniu. Wszystkie współrzędne
/// liczone są ręcznie przez <see cref="MapViewport"/>, więc render i hit-testing są spójne.
/// </summary>
public sealed class MapDrawable(
    MapData map,
    MapViewport viewport,
    IMapInteractionState state) : IDrawable
{
    private readonly IReadOnlyList<RouteVisual> _routes = BuildRouteVisuals(map);

    /// <summary>Opcjonalny podkład (skan planszy) rysowany jako pierwsza warstwa.</summary>
    public Microsoft.Maui.Graphics.IImage? Background { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawBackground(canvas);

        foreach (var route in _routes)
        {
            DrawRoute(canvas, route);
        }

        foreach (var city in map.Cities)
        {
            DrawCity(canvas, city);
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

    private void DrawRoute(ICanvas canvas, RouteVisual route)
    {
        var routeState = state.GetRouteState(route.Id);
        var fill = routeState == RouteState.Done
            ? RouteColorPalette.Player
            : RouteColorPalette.ForRoute(route.Color);

        var bandThickness = (float)(2 * MapMetrics.WagonHalfWidth * viewport.Scale);
        var corner = bandThickness * 0.25f;

        foreach (var wagon in route.Wagons)
        {
            var center = viewport.MapToScreen(wagon.CenterX, wagon.CenterY);
            var length = (float)(wagon.Length * (1 - MapMetrics.WagonGapFraction) * viewport.Scale);

            canvas.SaveState();
            canvas.Translate(center.X, center.Y);
            canvas.Rotate((float)(wagon.Angle * 180.0 / Math.PI));

            var x = -length / 2;
            var y = -bandThickness / 2;

            canvas.FillColor = fill;
            canvas.FillRoundedRectangle(x, y, length, bandThickness, corner);

            // Obrys: zawsze cienki (czytelność wagonu), pogrubiony dla trasy zaznaczonej.
            if (routeState == RouteState.Selected)
            {
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = Math.Max(2f, bandThickness * 0.28f);
            }
            else
            {
                canvas.StrokeColor = Color.FromArgb("#33000000");
                canvas.StrokeSize = 1f;
            }

            canvas.DrawRoundedRectangle(x, y, length, bandThickness, corner);
            canvas.RestoreState();
        }
    }

    private void DrawCity(ICanvas canvas, City city)
    {
        var center = viewport.MapToScreen(city.X, city.Y);
        var radius = (float)(MapMetrics.CityRadius * viewport.Scale);

        canvas.FillColor = Color.FromArgb("#B71C1C");
        canvas.FillCircle(center.X, center.Y, radius);

        canvas.StrokeColor = Color.FromArgb("#3E2723");
        canvas.StrokeSize = Math.Max(1.5f, radius * 0.12f);
        canvas.DrawCircle(center.X, center.Y, radius);

        if (state.IsCityMarked(city.Id))
        {
            var ringRadius = radius + (float)(MapMetrics.CityMarkRingWidth * viewport.Scale);
            canvas.StrokeColor = Color.FromArgb("#FFC107");
            canvas.StrokeSize = (float)(MapMetrics.CityMarkRingWidth * viewport.Scale);
            canvas.DrawCircle(center.X, center.Y, ringRadius);
        }

        if (viewport.Scale >= MapMetrics.LabelMinScale)
        {
            canvas.FontColor = Color.FromArgb("#212121");
            canvas.FontSize = 12;
            canvas.DrawString(city.Name, center.X, center.Y - radius - 14, HorizontalAlignment.Center);
        }
    }

    private static IReadOnlyList<RouteVisual> BuildRouteVisuals(MapData map)
    {
        var cities = map.Cities.ToDictionary(c => c.Id);
        var visuals = new List<RouteVisual>(map.Routes.Count);
        foreach (var route in map.Routes)
        {
            var polyline = RouteGeometry.BuildPolyline(route, cities);
            var wagons = RouteGeometry.BuildWagons(polyline, route.WagonCount, MapMetrics.RouteEndMargin);
            visuals.Add(new RouteVisual(route.Id, route.Color, wagons));
        }

        return visuals;
    }

    private sealed record RouteVisual(string Id, RouteColor Color, IReadOnlyList<WagonPlacement> Wagons);
}

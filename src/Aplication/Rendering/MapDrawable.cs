namespace Aplication.Rendering;

/// <summary>
/// Rysuje całą planszę w jednym przebiegu: tło (opcjonalny podkład) → trasy → miasta →
/// oznaczenia stanów → etykiety. Nie przechowuje stanu interakcji — odpytuje
/// <see cref="IMapInteractionState"/> przy każdym rysowaniu. Wszystkie współrzędne
/// liczone są ręcznie przez <see cref="MapViewport"/>, więc render i hit-testing są spójne.
/// </summary>
public sealed class MapDrawable(
    MapData map,
    MapViewport viewport,
    IMapInteractionState state) : IDrawable
{
    public Microsoft.Maui.Graphics.IImage? Background { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawBackground(canvas);

        foreach (var route in map.Routes)
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

    private void DrawRoute(ICanvas canvas, Route route)
    {
        var routeState = state.GetRouteState(route.Id);

        if (routeState == RouteState.None)
        {
            return;
        }

        var color = routeState == RouteState.Done
            ? RouteColorPalette.Player
            : RouteColorPalette.ForRoute(route.Color);

        var thickness = (float)(2 * MapMetrics.WagonHalfWidth * viewport.Scale);

        var path = new PathF();
        var first = viewport.MapToScreen(route.Points[0]);
        path.MoveTo(first.X, first.Y);
        for (var i = 1; i < route.Points.Count; i++)
        {
            var p = viewport.MapToScreen(route.Points[i]);
            path.LineTo(p.X, p.Y);
        }

        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        // Trasa zaznaczona: biała poświata pod spodem jako podświetlenie.
        if (routeState == RouteState.Selected)
        {
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = thickness + Math.Max(4f, thickness * 0.6f);
            canvas.DrawPath(path);
        }

        canvas.StrokeColor = color;
        canvas.StrokeSize = thickness;
        canvas.DrawPath(path);
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
}

namespace Aplication.Rendering;

/// <summary>
/// Rysuje całą planszę w jednym przebiegu: tło (opcjonalny podkład) → trasy → miasta. Nie przechowuje
/// stanu interakcji — przy każdym rysowaniu odpytuje <see cref="IMapInteractionState"/> o stan trasy
/// i oznaczenie miasta. Wszystkie współrzędne liczy <see cref="MapViewport"/>, więc rysowanie i
/// hit-testing pozostają spójne.
/// </summary>
public sealed class MapDrawable(MapData map, MapViewport viewport, IMapInteractionState state) : IDrawable
{
    private static readonly Color CityMarkColor = Color.FromArgb("#EC407A");

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

        // Domyślnie trasa jest przezroczysta — kolor trasy widać z podkładu (tła).
        if (routeState == RouteState.None)
        {
            return;
        }

        canvas.StrokeLineJoin = LineJoin.Miter;
        canvas.StrokeLineCap = LineCap.Butt;

        foreach (var wagon in route.Wagons)
        {
            DrawWagon(canvas, wagon, routeState);
        }
    }

    private void DrawWagon(ICanvas canvas, WagonRectangle wagon, RouteState routeState)
    {
        var corners = wagon.Corners.Select(viewport.MapToScreen).ToArray();
        var path = new PathF();
        path.MoveTo(corners[0]);
        for (var i = 1; i < corners.Length; i++)
        {
            path.LineTo(corners[i]);
        }

        path.Close();

        var playerColor = RouteColorPalette.ToColor(state.WagonColor);

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

    private void DrawCity(ICanvas canvas, City city)
    {
        // Bez oznaczenia miasto jest przezroczyste (nic nie rysujemy).
        if (!state.IsCityMarked(city.Id))
        {
            return;
        }

        // Oznaczone: wypełniony punkt w kolorze akcentu.
        var center = viewport.MapToScreen(city.X, city.Y);
        var radius = (float)(MapMetrics.CityRadius * viewport.Scale);
        canvas.FillColor = CityMarkColor;
        canvas.FillCircle(center.X, center.Y, radius);
    }
}

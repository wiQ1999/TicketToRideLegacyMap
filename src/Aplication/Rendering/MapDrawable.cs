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

        if (routeState == RouteState.Done)
        {
            // Wykonany: wypełnienie kolorem gracza + ukośne paski (45°) w ciemniejszym odcieniu,
            // co wyróżnia go od nieoznaczonego wagonika w tym samym kolorze.
            canvas.FillColor = RouteColorPalette.Player;
            canvas.FillPath(path);

            canvas.StrokeColor = Color.FromArgb("#1B5E20");
            canvas.StrokeSize = 2f;
            DrawDiagonalHatch(canvas, path, corners, 8f);

            canvas.StrokeSize = 1.5f;
            canvas.DrawPath(path);
        }
        else
        {
            // Zaznaczony: samo obramowanie (różowy obrys z ciemną krawędzią), wnętrze przezroczyste.
            canvas.StrokeColor = Color.FromArgb("#AD1457");
            canvas.StrokeSize = 3.5f;
            canvas.DrawPath(path);

            canvas.StrokeColor = Color.FromArgb("#EC407A");
            canvas.StrokeSize = 2f;
            canvas.DrawPath(path);
        }
    }

    private void DrawCity(ICanvas canvas, City city)
    {
        // Bez zaznaczenia miasto jest przezroczyste (nic nie rysujemy).
        if (!state.IsCityMarked(city.Id))
        {
            return;
        }

        // Zaznaczone: różowe wypełnienie, bez obramowania.
        var center = viewport.MapToScreen(city.X, city.Y);
        var radius = (float)(MapMetrics.CityRadius * viewport.Scale);
        canvas.FillColor = Color.FromArgb("#EC407A");
        canvas.FillCircle(center.X, center.Y, radius);
    }

    // Ukośne kreski pod kątem 45° (w przestrzeni ekranu), przycięte do konturu wagonika —
    // niezależnie od tego, pod jakim kątem sam wagonik jest obrócony.
    private static void DrawDiagonalHatch(ICanvas canvas, PathF path, IReadOnlyList<PointF> corners, float spacing)
    {
        var minX = corners.Min(p => p.X);
        var maxX = corners.Max(p => p.X);
        var minY = corners.Min(p => p.Y);
        var maxY = corners.Max(p => p.Y);
        var w = maxX - minX;
        var h = maxY - minY;
        var reach = w + h;

        canvas.SaveState();
        canvas.ClipPath(path);

        for (var c = -reach; c <= reach; c += spacing)
        {
            canvas.DrawLine(minX + c, minY, minX + c + h, minY + h);
        }

        canvas.RestoreState();
    }
}

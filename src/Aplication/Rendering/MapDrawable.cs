namespace Aplication.Rendering;

/// <summary>
/// Rysuje całą planszę w jednym przebiegu: tło (opcjonalny podkład) → trasy → miasta. Na tym etapie
/// widok służy wyłącznie do wyświetlania — renderer nie odpytuje żadnego stanu interakcji, pokazuje
/// niemutowalną geometrię bazowej mapy. Wszystkie współrzędne liczy <see cref="MapViewport"/>, więc
/// rysowanie i przyszły hit-testing pozostaną spójne.
/// </summary>
public sealed class MapDrawable(MapData map, MapViewport viewport) : IDrawable
{
    private static readonly Color GeometryColor = Color.FromArgb("#5D4037");

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
        canvas.StrokeLineJoin = LineJoin.Miter;
        canvas.StrokeLineCap = LineCap.Butt;

        foreach (var wagon in route.Wagons)
        {
            DrawWagon(canvas, wagon);
        }
    }

    private void DrawWagon(ICanvas canvas, WagonRectangle wagon)
    {
        var corners = wagon.Corners.Select(viewport.MapToScreen).ToArray();
        var path = new PathF();
        path.MoveTo(corners[0]);
        for (var i = 1; i < corners.Length; i++)
        {
            path.LineTo(corners[i]);
        }

        path.Close();

        // Podgląd geometrii: sam obrys wagonika nad podkładem — bez wypełnienia i bez stanu.
        canvas.StrokeColor = GeometryColor;
        canvas.StrokeSize = 2f;
        canvas.DrawPath(path);
    }

    private void DrawCity(ICanvas canvas, City city)
    {
        var center = viewport.MapToScreen(city.X, city.Y);
        var radius = (float)(MapMetrics.CityRadius * viewport.Scale);

        // Podgląd geometrii: obrys znacznika miasta nad podkładem — bez stanu oznaczenia.
        canvas.StrokeColor = GeometryColor;
        canvas.StrokeSize = 2f;
        canvas.DrawCircle(center.X, center.Y, radius);
    }
}

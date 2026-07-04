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

        var halfWidth = (float)(MapMetrics.WagonHalfWidth * viewport.Scale);
        var points = new PointF[route.Points.Count];
        for (var i = 0; i < points.Length; i++)
        {
            points[i] = viewport.MapToScreen(route.Points[i]);
        }

        // Pasmo jako kontur z prostymi (kwadratowymi) końcami — wnętrze pozostaje przezroczyste.
        var band = BuildBandOutline(points, halfWidth);
        canvas.StrokeLineJoin = LineJoin.Miter;
        canvas.StrokeLineCap = LineCap.Butt;

        if (routeState == RouteState.Done)
        {
            // Wykonana: wypełnienie kolorem gracza + ukośne paski (45° do kierunku trasy)
            // w ciemniejszym odcieniu, co wyróżnia ją od nieoznaczonej trasy w tym samym kolorze.
            canvas.FillColor = RouteColorPalette.Player;
            canvas.FillPath(band);

            var darker = Color.FromArgb("#1B5E20");
            canvas.StrokeColor = darker;
            canvas.StrokeSize = Math.Max(2f, halfWidth * 0.5f);
            DrawDiagonalHatch(canvas, points, halfWidth, Math.Max(6f, halfWidth * 1.1f));

            canvas.StrokeSize = Math.Max(2f, halfWidth * 0.35f);
            canvas.DrawPath(band);
        }
        else
        {
            // Zaznaczona: samo obramowanie (różowy obrys z ciemną krawędzią), wnętrze przezroczyste.
            canvas.StrokeColor = Color.FromArgb("#AD1457");
            canvas.StrokeSize = Math.Max(3.5f, halfWidth * 0.6f);
            canvas.DrawPath(band);

            canvas.StrokeColor = Color.FromArgb("#EC407A");
            canvas.StrokeSize = Math.Max(2f, halfWidth * 0.3f);
            canvas.DrawPath(band);
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

    // Ukośne kreski wzdłuż trasy, pod kątem 45° do lokalnego kierunku każdego segmentu.
    // Każdy segment sztrychuje się osobno, z przycięciem do jego prostokątnego pasma.
    private static void DrawDiagonalHatch(
        ICanvas canvas, IReadOnlyList<PointF> points, float halfWidth, float spacing)
    {
        const float cos45 = 0.70710677f;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var a = points[i];
            var b = points[i + 1];
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var len = MathF.Sqrt((dx * dx) + (dy * dy));
            if (len < 1e-4f)
            {
                continue;
            }

            dx /= len;
            dy /= len;

            // Normalna segmentu — czworobok pasma dla tego odcinka.
            var nx = -dy * halfWidth;
            var ny = dx * halfWidth;
            var quad = new PathF();
            quad.MoveTo(a.X + nx, a.Y + ny);
            quad.LineTo(b.X + nx, b.Y + ny);
            quad.LineTo(b.X - nx, b.Y - ny);
            quad.LineTo(a.X - nx, a.Y - ny);
            quad.Close();

            // Kierunek kresek: kierunek segmentu obrócony o 45°; p — wektor rozstawu (prostopadły).
            var hx = cos45 * (dx - dy);
            var hy = cos45 * (dx + dy);
            var px = -hy;
            var py = hx;

            var cx = (a.X + b.X) / 2f;
            var cy = (a.Y + b.Y) / 2f;
            var reach = (len / 2f) + halfWidth;

            canvas.SaveState();
            canvas.ClipPath(quad);
            for (var c = -reach; c <= reach; c += spacing)
            {
                var mx = cx + (px * c);
                var my = cy + (py * c);
                canvas.DrawLine(mx - (hx * reach), my - (hy * reach), mx + (hx * reach), my + (hy * reach));
            }

            canvas.RestoreState();
        }
    }

    // Zamknięty kontur pasma trasy (offset łamanej o ±halfWidth) z prostymi końcami. Pozwala
    // rysować samo obramowanie (przezroczyste wnętrze) lub wypełnić pasmo kolorem.
    private static PathF BuildBandOutline(IReadOnlyList<PointF> points, float halfWidth)
    {
        var n = points.Count;

        var dirX = new float[n - 1];
        var dirY = new float[n - 1];
        for (var i = 0; i < n - 1; i++)
        {
            var dx = points[i + 1].X - points[i].X;
            var dy = points[i + 1].Y - points[i].Y;
            var len = MathF.Sqrt((dx * dx) + (dy * dy));
            if (len < 1e-4f)
            {
                dx = 1f;
                dy = 0f;
                len = 1f;
            }

            dirX[i] = dx / len;
            dirY[i] = dy / len;
        }

        // Wektor odsunięcia w każdym wierzchołku: normalna, z zaostrzeniem miter na zgięciach.
        var offX = new float[n];
        var offY = new float[n];
        for (var i = 0; i < n; i++)
        {
            float nx, ny;
            if (i == 0)
            {
                nx = -dirY[0];
                ny = dirX[0];
            }
            else if (i == n - 1)
            {
                nx = -dirY[n - 2];
                ny = dirX[n - 2];
            }
            else
            {
                var mx = -dirY[i - 1] - dirY[i];
                var my = dirX[i - 1] + dirX[i];
                var mlen = MathF.Sqrt((mx * mx) + (my * my));
                if (mlen < 1e-4f)
                {
                    nx = -dirY[i];
                    ny = dirX[i];
                }
                else
                {
                    mx /= mlen;
                    my /= mlen;
                    var cos = (mx * -dirY[i]) + (my * dirX[i]); // cos połowy kąta
                    var scale = cos > 0.2f ? 1f / cos : 5f; // ogranicz miter przy ostrych kątach
                    nx = mx * scale;
                    ny = my * scale;
                }
            }

            offX[i] = nx * halfWidth;
            offY[i] = ny * halfWidth;
        }

        var path = new PathF();
        path.MoveTo(points[0].X + offX[0], points[0].Y + offY[0]);
        for (var i = 1; i < n; i++)
        {
            path.LineTo(points[i].X + offX[i], points[i].Y + offY[i]);
        }

        for (var i = n - 1; i >= 0; i--)
        {
            path.LineTo(points[i].X - offX[i], points[i].Y - offY[i]);
        }

        path.Close();
        return path;
    }
}

namespace Aplication.Models;

/// <summary>
/// Prostokąt pojedynczego wagonika trasy, zdefiniowany dwoma przeciwległymi rogami przekątnej
/// (<see cref="A"/>, <see cref="B"/>) w przestrzeni mapy — może być obrócony pod dowolnym kątem.
/// Pozostałe dwa rogi wyliczane są z długości przekątnej i stałego krótszego boku
/// (<see cref="ShortSide"/>): dłuższy bok wynika z twierdzenia Pitagorasa.
/// </summary>
public readonly record struct WagonRectangle(MapPoint A, MapPoint B)
{
    // Stała długość krótszego boku wagonika w przestrzeni mapy — wspólna dla wszystkich wagoników.
    public const double ShortSide = 16.0;

    public MapPoint Center => new((A.X + B.X) / 2.0, (A.Y + B.Y) / 2.0);

    // Cztery rogi prostokąta w kolejności obwodu: A, A+krótszy bok, B, B−krótszy bok.
    public IReadOnlyList<MapPoint> Corners
    {
        get
        {
            // Przekątna A→B ma długość d; krótszy bok jest stały (ShortSide = w), dłuższy L = √(d²−w²).
            // Krótki bok przy rogu A to wektor v o długości w, odchylony od przekątnej o kąt α, gdzie
            // cos α = w/d (rzut jednostkowej przekątnej na kierunek krótkiego boku). Pozostałe rogi to
            // A+v oraz B−v; dłuższy bok wychodzi jako AB−v.
            var dx = B.X - A.X;
            var dy = B.Y - A.Y;
            var diagonal = Math.Sqrt((dx * dx) + (dy * dy));

            var cos = ShortSide / diagonal;             // = w / d
            var sin = Math.Sqrt(1.0 - (cos * cos));     // = L / d

            // v = w · (jednostkowa przekątna obrócona o +α); |v| = ShortSide.
            var vx = (ShortSide / diagonal) * ((dx * cos) - (dy * sin));
            var vy = (ShortSide / diagonal) * ((dx * sin) + (dy * cos));

            var d = new MapPoint(A.X + vx, A.Y + vy);
            var c = new MapPoint(B.X - vx, B.Y - vy);
            return [A, d, B, c];
        }
    }

    public bool Contains(MapPoint p) => IsInsideConvexPolygon(p, Corners);

    private static bool IsInsideConvexPolygon(MapPoint p, IReadOnlyList<MapPoint> corners)
    {
        var sign = 0;
        for (var i = 0; i < corners.Count; i++)
        {
            var p1 = corners[i];
            var p2 = corners[(i + 1) % corners.Count];
            var cross = ((p2.X - p1.X) * (p.Y - p1.Y)) - ((p2.Y - p1.Y) * (p.X - p1.X));
            var side = Math.Sign(cross);
            if (side == 0)
            {
                continue;
            }

            if (sign == 0)
            {
                sign = side;
            }
            else if (side != sign)
            {
                return false;
            }
        }

        return true;
    }
}

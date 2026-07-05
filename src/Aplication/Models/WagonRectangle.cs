namespace Aplication.Models;

/// <summary>
/// Kwadrat pojedynczego wagonika trasy, zdefiniowany dwoma przeciwległymi rogami przekątnej
/// (<see cref="A"/>, <see cref="B"/>) w przestrzeni mapy — może być obrócony pod dowolnym kątem.
/// Pozostałe dwa rogi wyliczane są z założenia kątów prostych: druga przekątna ma ten sam środek
/// i długość co <see cref="A"/>-<see cref="B"/>, obróconą o 90°.
/// </summary>
public readonly record struct WagonRectangle(MapPoint A, MapPoint B)
{
    public MapPoint Center => new((A.X + B.X) / 2.0, (A.Y + B.Y) / 2.0);

    /// <summary>Cztery rogi w kolejności obwodu (A, róg drugiej przekątnej, B, przeciwny róg drugiej przekątnej).</summary>
    public IReadOnlyList<MapPoint> Corners
    {
        get
        {
            var center = Center;
            var dx = B.X - center.X;
            var dy = B.Y - center.Y;
            var c = new MapPoint(center.X - dy, center.Y + dx);
            var d = new MapPoint(center.X + dy, center.Y - dx);
            return [A, c, B, d];
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

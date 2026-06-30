namespace Aplication.Models;

/// <summary>
/// Punkt w przestrzeni mapy (logicznej, niezależnej od ekranu i poziomu zoomu).
/// Używany dla pozycji oraz punktów pośrednich tras (waypoints).
/// </summary>
public readonly record struct MapPoint(double X, double Y);

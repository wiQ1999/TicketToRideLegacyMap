using System.Text;

namespace Aplication.Services;

/// <summary>
/// Domyślna, in-memory implementacja edytora danych mapy trybu deweloperskiego. Trzyma mutowalne
/// kopie miast i tras; identyfikatory nowych miast generuje z nazwy, zapewniając unikalność.
/// </summary>
public sealed class DeveloperMapEditor : IDeveloperMapEditor
{
    private readonly List<City> _cities = [];
    private readonly List<Route> _routes = [];

    public IReadOnlyList<City> Cities => _cities;

    public IReadOnlyList<Route> Routes => _routes;

    public void LoadFrom(MapData map)
    {
        _cities.Clear();
        _routes.Clear();
        _cities.AddRange(map.Cities);
        _routes.AddRange(map.Routes);
    }

    public City AddCity(string name, MapPoint position)
    {
        var city = new City(GenerateId(name), name, position.X, position.Y);
        _cities.Add(city);
        return city;
    }

    public void UpdateCity(string id, string name, MapPoint position)
    {
        var index = _cities.FindIndex(c => c.Id == id);
        if (index < 0)
        {
            return;
        }

        _cities[index] = new City(id, name, position.X, position.Y);
    }

    public void RemoveCity(string id) => _cities.RemoveAll(c => c.Id == id);

    public Route AddRoute(string cityFromId, string cityToId, IReadOnlyList<WagonRectangle> wagons)
    {
        var route = new Route(GenerateRouteId(cityFromId, cityToId), cityFromId, cityToId, wagons);
        _routes.Add(route);
        return route;
    }

    public void UpdateRoute(string id, string cityFromId, string cityToId, IReadOnlyList<WagonRectangle> wagons)
    {
        var index = _routes.FindIndex(r => r.Id == id);
        if (index < 0)
        {
            return;
        }

        _routes[index] = new Route(id, cityFromId, cityToId, wagons);
    }

    public void RemoveRoute(string id) => _routes.RemoveAll(r => r.Id == id);

    // Bazowy identyfikator to wersaliki liter/cyfr z nazwy; przy kolizji dodawany jest sufiks liczbowy.
    private string GenerateId(string name)
    {
        var builder = new StringBuilder();
        foreach (var ch in name)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToUpperInvariant(ch));
            }
        }

        var baseId = builder.Length > 0 ? builder.ToString() : "CITY";
        var id = baseId;
        var suffix = 2;
        while (_cities.Any(c => c.Id == id))
        {
            id = $"{baseId}-{suffix++}";
        }

        return id;
    }

    // Bazowy identyfikator to para miast końcowych; przy kolizji dodawany jest sufiks liczbowy.
    private string GenerateRouteId(string cityFromId, string cityToId)
    {
        var baseId = $"{cityFromId}-{cityToId}";
        var id = baseId;
        var suffix = 2;
        while (_routes.Any(r => r.Id == id))
        {
            id = $"{baseId}-{suffix++}";
        }

        return id;
    }
}

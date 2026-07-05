namespace Aplication.Services;

/// <summary>
/// Wbudowany katalog nazw miast z gry „Wsiąść do pociągu: Legendy zachodu". Lista jest stała
/// i wykorzystywana jako źródło podpowiedzi przy dodawaniu miast w trybie deweloperskim.
/// </summary>
public sealed class CityNameCatalog : ICityNameCatalog
{
    private static readonly string[] Names =
    [
        "Albany", "Atlanta", "Baja", "Baltimore", "Bangor", "Boston", "Buffalo", "Calgary",
        "Cemetery City", "Charleston", "Charlotte", "Cheyenne", "Chicago", "Chihuahua", "Cincinnati",
        "Dallas", "Davenport", "Denver", "Dodge City", "Duluth", "El Paso", "Fargo", "Helena",
        "Hermosillo", "Houston", "Jacksonville", "Kansas City", "Knoxville", "Lewisburg", "Little Rock",
        "Miami", "Miles City", "Mobile", "Monterrey", "Montreal", "Nashville", "New Orleans", "New York",
        "Nuevos Angeles", "Oklahoma City", "Omaha", "Pacific Haven", "Philadelphia", "Phoenix",
        "Pittsburgh", "Portland", "Quebec", "Regina", "Sacramento", "Salt Lake City", "San Antonio",
        "San Francisco", "Santa Fe", "Savannah", "Seattle", "Spokane", "St. Louis", "St. Paul", "Tampa",
        "Vancouver", "Winnipeg", "Norfolk"
    ];

    public IReadOnlyList<string> AllNames { get; } =
        Names.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase).ToArray();

    public IReadOnlyList<string> Suggest(string query, int max)
    {
        var trimmed = query?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return [];
        }

        return AllNames
            .Where(n => n.Contains(trimmed, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(n => n.StartsWith(trimmed, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
            .ThenBy(n => n, StringComparer.CurrentCultureIgnoreCase)
            .Take(max)
            .ToArray();
    }

    public string? Resolve(string name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return AllNames.FirstOrDefault(n => string.Equals(n, trimmed, StringComparison.CurrentCultureIgnoreCase));
    }
}

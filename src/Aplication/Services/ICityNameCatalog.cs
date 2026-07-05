namespace Aplication.Services;

/// <summary>
/// Stała, wbudowana lista nazw miast z fizycznej gry. Źródło podpowiedzi przy dodawaniu miast
/// w trybie deweloperskim; nazwy miast są wybierane wyłącznie z tego zbioru.
/// </summary>
public interface ICityNameCatalog
{
    /// <summary>Wszystkie dostępne nazwy miast, w kolejności alfabetycznej.</summary>
    IReadOnlyList<string> AllNames { get; }

    /// <summary>Podpowiedzi nazw pasujących do <paramref name="query"/> (maks. <paramref name="max"/>).</summary>
    IReadOnlyList<string> Suggest(string query, int max);

    /// <summary>Zwraca kanoniczną nazwę z katalogu równą (bez względu na wielkość liter) podanej, albo <c>null</c>.</summary>
    string? Resolve(string name);
}

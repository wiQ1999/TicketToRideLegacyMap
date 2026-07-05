namespace Aplication.Services;

/// <summary>
/// Robocze, mutowalne listy miast i tras trybu deweloperskiego, trzymane w pamięci osobno od
/// niemutowalnej mapy bazowej. Inicjalizowane przy wejściu w tryb danymi z <see cref="IMapDataProvider"/>;
/// jedynym trwałym efektem pracy jest eksport (osobny etap), nie zapis do mapy bazowej.
/// </summary>
public interface IDeveloperMapEditor
{
    /// <summary>Bieżąca robocza lista miast.</summary>
    IReadOnlyList<City> Cities { get; }

    /// <summary>Bieżąca robocza lista tras.</summary>
    IReadOnlyList<Route> Routes { get; }

    /// <summary>Zastępuje robocze listy kopią danych z <paramref name="map"/> (wywoływane przy wejściu w tryb).</summary>
    void LoadFrom(MapData map);

    /// <summary>Dodaje nowe miasto o wskazanej nazwie i położeniu; zwraca utworzony wpis.</summary>
    City AddCity(string name, MapPoint position);

    /// <summary>Aktualizuje nazwę i położenie istniejącego miasta (po <paramref name="id"/>).</summary>
    void UpdateCity(string id, string name, MapPoint position);

    /// <summary>Usuwa miasto o wskazanym <paramref name="id"/> z listy roboczej.</summary>
    void RemoveCity(string id);
}

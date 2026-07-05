namespace Aplication.Services;

/// <summary>
/// Serializuje robocze dane mapy trybu deweloperskiego (miasta i trasy z <see cref="IDeveloperMapEditor"/>)
/// do formatu zgodnego z <c>mapa.json</c> i kopiuje wynik do schowka systemowego, do dalszego ręcznego
/// wykorzystania (np. wklejenia do plików danych aplikacji).
/// </summary>
public interface IMapDataExporter
{
    /// <summary>Serializuje podane dane mapy do sformatowanego tekstu JSON zgodnego ze schematem <c>mapa.json</c>.</summary>
    string ExportToJson(MapSize canvasSize, IReadOnlyList<City> cities, IReadOnlyList<Route> routes);

    /// <summary>Serializuje podane dane mapy i kopiuje wynik do schowka systemowego (<c>Clipboard</c>, MAUI Essentials).</summary>
    Task ExportToClipboardAsync(MapSize canvasSize, IReadOnlyList<City> cities, IReadOnlyList<Route> routes);
}

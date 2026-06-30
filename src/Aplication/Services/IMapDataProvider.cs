namespace Aplication.Services;

/// <summary>
/// Dostarcza statyczny układ planszy (<see cref="MapData"/>) wczytany z osadzonego zasobu.
/// Kod renderujący zależy wyłącznie od tego interfejsu i modeli, dzięki czemu podmiana
/// danych placeholder na pełną planszę nie wymaga zmian w rendererze — wystarczy podmienić
/// plik z danymi.
/// </summary>
public interface IMapDataProvider
{
    /// <summary>
    /// Zwraca dane mapy. Wynik jest wczytywany raz i buforowany — kolejne wywołania
    /// zwracają tę samą instancję.
    /// </summary>
    Task<MapData> GetMapDataAsync();
}

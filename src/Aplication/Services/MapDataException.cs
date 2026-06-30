namespace Aplication.Services;

/// <summary>
/// Błąd wczytania lub walidacji danych mapy. Dane mapy są wbudowane w aplikację,
/// więc taki błąd oznacza wadliwe dane buildu, a nie błąd działania po stronie użytkownika.
/// </summary>
public sealed class MapDataException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
}

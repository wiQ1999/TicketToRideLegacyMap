namespace Aplication.Services;

/// <summary>
/// Trzyma zmienny stan interakcji nałożony na niemutowalną mapę bazową: oznaczenia miast
/// (toggle) oraz stany tras (cykl <see cref="RouteState"/>). Renderer odpytuje ten serwis
/// przy każdym rysowaniu — sam nie przechowuje stanu. Zmiana stanu zgłaszana jest przez
/// <see cref="Changed"/>, na które reaguje widok, wywołując ponowne rysowanie.
/// </summary>
public interface IMapInteractionState
{
    /// <summary>Zgłaszane po każdej zmianie stanu (toggle miasta lub cykl trasy).</summary>
    event EventHandler? Changed;

    /// <summary>Czy miasto jest oznaczone.</summary>
    bool IsCityMarked(string cityId);

    /// <summary>Przełącza oznaczenie miasta i zgłasza <see cref="Changed"/>.</summary>
    void ToggleCity(string cityId);

    /// <summary>Zwraca stan trasy (domyślnie <see cref="RouteState.None"/>).</summary>
    RouteState GetRouteState(string routeId);

    /// <summary>Przechodzi trasę o krok w cyklu None→Selected→Done→None i zgłasza <see cref="Changed"/>.</summary>
    void CycleRoute(string routeId);
}

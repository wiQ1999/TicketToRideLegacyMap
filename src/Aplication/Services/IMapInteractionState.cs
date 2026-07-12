namespace Aplication.Services;

/// <summary>
/// Trzyma zmienny stan interakcji nałożony na niemutowalną mapę bazową: oznaczenia miast
/// (toggle), stany tras (cykl <see cref="RouteState"/>) i wybrany kolor wagonów gracza.
/// Renderer odpytuje ten serwis przy każdym rysowaniu — sam nie przechowuje stanu. Zmiana
/// stanu zgłaszana jest przez <see cref="Changed"/>, na które reaguje widok, wywołując
/// ponowne rysowanie.
/// </summary>
public interface IMapInteractionState
{
    /// <summary>Zgłaszane po każdej zmianie stanu (toggle miasta, cykl trasy, kolor, reset).</summary>
    event EventHandler? Changed;

    /// <summary>Aktualnie wybrany kolor wagonów gracza.</summary>
    WagonColor WagonColor { get; }

    /// <summary>Czy w tej sesji rozpoczęto już rozgrywkę (umożliwia „Kontynuuj").</summary>
    bool HasActivePlan { get; }

    /// <summary>Czy miasto jest oznaczone.</summary>
    bool IsCityMarked(string cityId);

    /// <summary>Przełącza oznaczenie miasta i zgłasza <see cref="Changed"/>.</summary>
    void ToggleCity(string cityId);

    /// <summary>Zwraca stan trasy (domyślnie <see cref="RouteState.None"/>).</summary>
    RouteState GetRouteState(string routeId);

    /// <summary>Przechodzi trasę o krok w cyklu None→Selected→Done→None i zgłasza <see cref="Changed"/>.</summary>
    void CycleRoute(string routeId);

    /// <summary>Ustawia kolor wagonów gracza i zgłasza <see cref="Changed"/>.</summary>
    void SetWagonColor(WagonColor color);

    /// <summary>Rozpoczyna nową rozgrywkę: czyści oznaczenia, ustawia kolor, oznacza plan jako aktywny.</summary>
    void StartNewPlan(WagonColor color);
}

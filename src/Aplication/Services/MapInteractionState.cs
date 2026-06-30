namespace Aplication.Services;

/// <summary>
/// Domyślna, in-memory implementacja stanu interakcji. Brak persystencji — stan żyje
/// tyle, co sesja (zgodnie z zakresem PoC). Mapa bazowa pozostaje niemutowalna.
/// </summary>
public sealed class MapInteractionState : IMapInteractionState
{
    private readonly HashSet<string> _markedCities = [];
    private readonly Dictionary<string, RouteState> _routeStates = [];

    public event EventHandler? Changed;

    public bool IsCityMarked(string cityId) => _markedCities.Contains(cityId);

    public void ToggleCity(string cityId)
    {
        if (!_markedCities.Add(cityId))
        {
            _markedCities.Remove(cityId);
        }

        RaiseChanged();
    }

    public RouteState GetRouteState(string routeId) =>
        _routeStates.TryGetValue(routeId, out var state) ? state : RouteState.None;

    public void CycleRoute(string routeId)
    {
        _routeStates[routeId] = GetRouteState(routeId) switch
        {
            RouteState.None => RouteState.Selected,
            RouteState.Selected => RouteState.Done,
            _ => RouteState.None
        };

        RaiseChanged();
    }

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}

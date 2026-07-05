using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aplication.PageModels;

/// <summary>Wiersz listy roboczych tras — trasa wraz z nazwami miast końcowych do wyświetlenia.</summary>
public sealed record DeveloperRouteRow(Route Route, string CityFromName, string CityToName)
{
    public int WagonCount => Route.WagonCount;
}

/// <summary>
/// Model trybu deweloperskiego. Przy wejściu ładuje aktualne dane mapy do roboczych list przez
/// <see cref="IDeveloperMapEditor"/>. Umożliwia dodawanie miasta (wskazane położenie na mapie + nazwa
/// z <see cref="ICityNameCatalog"/> z podpowiedziami), edycję i usuwanie miast, a także dodawanie tras
/// (dwa miasta z listy roboczej + wagoniki wskazywane na mapie parami punktów przekątnej) wraz z edycją
/// i usuwaniem pojedynczych wagoników oraz całych tras.
/// </summary>
public sealed partial class DeveloperPageModel(
    IMapDataProvider mapDataProvider,
    IDeveloperMapEditor editor,
    ICityNameCatalog cityNameCatalog,
    IErrorHandler errorHandler) : ObservableObject
{
    private const int MaxNameSuggestions = 6;

    private bool _suppressSuggestions;
    private bool _suppressRouteFromSuggestions;
    private bool _suppressRouteToSuggestions;
    private MapPoint? _pendingWagonFirstPoint;
    private int? _editingWagonIndex;

    /// <summary>Zgłaszane, gdy zmienia się nakładka mapy (miasta, wskazany punkt lub wagoniki roboczej trasy).</summary>
    public event EventHandler? OverlayChanged;

    public MapData? Map { get; private set; }

    public ObservableCollection<City> Cities { get; } = [];

    public ObservableCollection<DeveloperRouteRow> Routes { get; } = [];

    public ObservableCollection<WagonRectangle> PendingWagons { get; } = [];

    public ObservableCollection<string> NameSuggestions { get; } = [];

    public bool HasSuggestions => NameSuggestions.Count > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCitiesTab))]
    private bool _isRoutesTab;

    public bool IsCitiesTab => !IsRoutesTab;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddOrUpdateCityCommand))]
    private string _cityName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddOrUpdateCityCommand))]
    [NotifyPropertyChangedFor(nameof(PickedPositionText))]
    private MapPoint? _pickedPosition;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    [NotifyPropertyChangedFor(nameof(SubmitLabel))]
    private City? _editingCity;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveRouteCommand))]
    private string _routeCityFromText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveRouteCommand))]
    private string _routeCityToText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditingRoute))]
    [NotifyPropertyChangedFor(nameof(RouteSubmitLabel))]
    private Route? _editingRoute;

    public bool IsEditing => EditingCity is not null;

    public string SubmitLabel => IsEditing ? "Zapisz" : "Dodaj";

    public string PickedPositionText => PickedPosition is { } p
        ? $"Położenie: {p.X:0}, {p.Y:0}"
        : "Wskaż położenie na mapie";

    public bool IsEditingRoute => EditingRoute is not null;

    public string RouteSubmitLabel => IsEditingRoute ? "Zapisz trasę" : "Dodaj trasę";

    public string WagonPickHint => _pendingWagonFirstPoint is not null
        ? "Wskazano pierwszy róg wagonika - dotknij mapy, aby wskazać drugi róg."
        : "Dotknij mapy, aby wskazać pierwszy róg kolejnego wagonika.";

    /// <summary>Miasto początkowe rozwiązane z <see cref="RouteCityFromText"/> względem listy roboczej.</summary>
    public City? RouteCityFrom => ResolveCityByName(RouteCityFromText);

    /// <summary>Miasto końcowe rozwiązane z <see cref="RouteCityToText"/> względem listy roboczej.</summary>
    public City? RouteCityTo => ResolveCityByName(RouteCityToText);

    public ObservableCollection<string> RouteCityFromSuggestions { get; } = [];

    public ObservableCollection<string> RouteCityToSuggestions { get; } = [];

    public bool HasRouteCityFromSuggestions => RouteCityFromSuggestions.Count > 0;

    public bool HasRouteCityToSuggestions => RouteCityToSuggestions.Count > 0;

    /// <summary>Czy punkt oczekujący na nakładce to róg wagonika (trasy), a nie pozycja miasta.</summary>
    public bool IsOverlayPendingPointWagonCorner => IsRoutesTab;

    public async Task InitializeAsync()
    {
        try
        {
            Map = await mapDataProvider.GetMapDataAsync();
            editor.LoadFrom(Map);
            ClearForm();
            ClearRouteForm();
            RefreshCities();
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }

    /// <summary>Obsługuje dotknięcie planszy: w karcie miast wskazuje położenie, w karcie tras dodaje róg wagonika.</summary>
    public void HandleMapTap(MapPoint point)
    {
        if (IsRoutesTab)
        {
            HandleRouteWagonTap(point);
            return;
        }

        PickedPosition = point;
        OnOverlayChanged();
    }

    /// <summary>Punkt do narysowania jako wskazany-lecz-niezatwierdzony, zależnie od aktywnej karty.</summary>
    public MapPoint? OverlayPendingPoint => IsRoutesTab ? _pendingWagonFirstPoint : PickedPosition;

    [RelayCommand]
    private void SelectCitiesTab() => IsRoutesTab = false;

    [RelayCommand]
    private void SelectRoutesTab() => IsRoutesTab = true;

    [RelayCommand]
    private void SelectSuggestion(string name)
    {
        _suppressSuggestions = true;
        CityName = name;
        _suppressSuggestions = false;
        ClearSuggestions();
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void AddOrUpdateCity()
    {
        if (cityNameCatalog.Resolve(CityName) is not { } canonicalName || PickedPosition is not { } position)
        {
            return;
        }

        if (EditingCity is { } editing)
        {
            editor.UpdateCity(editing.Id, canonicalName, position);
        }
        else
        {
            editor.AddCity(canonicalName, position);
        }

        ClearForm();
        RefreshCities();
    }

    private bool CanSubmit() =>
        PickedPosition is not null && cityNameCatalog.Resolve(CityName) is not null;

    [RelayCommand]
    private void EditCity(City city)
    {
        EditingCity = city;
        _suppressSuggestions = true;
        CityName = city.Name;
        _suppressSuggestions = false;
        ClearSuggestions();
        PickedPosition = city.Position;
        OnOverlayChanged();
    }

    [RelayCommand]
    private void DeleteCity(City city)
    {
        editor.RemoveCity(city.Id);
        if (EditingCity?.Id == city.Id)
        {
            ClearForm();
        }

        RefreshCities();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ClearForm();
        OnOverlayChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSaveRoute))]
    private void SaveRoute()
    {
        if (RouteCityFrom is not { } from || RouteCityTo is not { } to)
        {
            return;
        }

        var wagons = PendingWagons.ToArray();
        if (EditingRoute is { } editing)
        {
            editor.UpdateRoute(editing.Id, from.Id, to.Id, wagons);
        }
        else
        {
            editor.AddRoute(from.Id, to.Id, wagons);
        }

        ClearRouteForm();
        RefreshRoutes();
    }

    private bool CanSaveRoute() =>
        RouteCityFrom is not null && RouteCityTo is not null &&
        RouteCityFrom.Id != RouteCityTo.Id && PendingWagons.Count > 0;

    [RelayCommand]
    private void SelectRouteCityFromSuggestion(string name) => SetRouteCityFromText(name);

    [RelayCommand]
    private void SelectRouteCityToSuggestion(string name) => SetRouteCityToText(name);

    [RelayCommand]
    private void EditRoute(DeveloperRouteRow row)
    {
        EditingRoute = row.Route;
        SetRouteCityFromText(GetCityName(row.Route.CityFromId));
        SetRouteCityToText(GetCityName(row.Route.CityToId));
        PendingWagons.Clear();
        foreach (var wagon in row.Route.Wagons)
        {
            PendingWagons.Add(wagon);
        }

        _pendingWagonFirstPoint = null;
        _editingWagonIndex = null;
        OnPropertyChanged(nameof(WagonPickHint));
        SaveRouteCommand.NotifyCanExecuteChanged();
        OnOverlayChanged();
    }

    [RelayCommand]
    private void DeleteRoute(DeveloperRouteRow row)
    {
        editor.RemoveRoute(row.Route.Id);
        if (EditingRoute?.Id == row.Route.Id)
        {
            ClearRouteForm();
        }

        RefreshRoutes();
    }

    [RelayCommand]
    private void CancelRouteEdit()
    {
        ClearRouteForm();
        OnOverlayChanged();
    }

    [RelayCommand]
    private void EditWagon(WagonRectangle wagon)
    {
        var index = PendingWagons.IndexOf(wagon);
        if (index < 0)
        {
            return;
        }

        _editingWagonIndex = index;
        _pendingWagonFirstPoint = null;
        OnPropertyChanged(nameof(WagonPickHint));
    }

    [RelayCommand]
    private void RemoveWagon(WagonRectangle wagon)
    {
        PendingWagons.Remove(wagon);
        _editingWagonIndex = null;
        _pendingWagonFirstPoint = null;
        OnPropertyChanged(nameof(WagonPickHint));
        SaveRouteCommand.NotifyCanExecuteChanged();
        OnOverlayChanged();
    }

    private void HandleRouteWagonTap(MapPoint point)
    {
        if (_pendingWagonFirstPoint is not { } first)
        {
            _pendingWagonFirstPoint = point;
            OnPropertyChanged(nameof(WagonPickHint));
            OnOverlayChanged();
            return;
        }

        var wagon = new WagonRectangle(first, point);
        if (_editingWagonIndex is { } index && index < PendingWagons.Count)
        {
            PendingWagons[index] = wagon;
            _editingWagonIndex = null;
        }
        else
        {
            PendingWagons.Add(wagon);
        }

        _pendingWagonFirstPoint = null;
        OnPropertyChanged(nameof(WagonPickHint));
        SaveRouteCommand.NotifyCanExecuteChanged();
        OnOverlayChanged();
    }

    partial void OnCityNameChanged(string value)
    {
        if (_suppressSuggestions)
        {
            return;
        }

        var matches = cityNameCatalog.Suggest(value, MaxNameSuggestions);
        NameSuggestions.Clear();
        foreach (var match in matches)
        {
            NameSuggestions.Add(match);
        }

        OnPropertyChanged(nameof(HasSuggestions));
    }

    partial void OnRouteCityFromTextChanged(string value)
    {
        if (_suppressRouteFromSuggestions)
        {
            return;
        }

        var matches = SuggestCityNames(value);
        RouteCityFromSuggestions.Clear();
        foreach (var match in matches)
        {
            RouteCityFromSuggestions.Add(match);
        }

        OnPropertyChanged(nameof(HasRouteCityFromSuggestions));
    }

    partial void OnRouteCityToTextChanged(string value)
    {
        if (_suppressRouteToSuggestions)
        {
            return;
        }

        var matches = SuggestCityNames(value);
        RouteCityToSuggestions.Clear();
        foreach (var match in matches)
        {
            RouteCityToSuggestions.Add(match);
        }

        OnPropertyChanged(nameof(HasRouteCityToSuggestions));
    }

    // Podpowiedzi nazw miast dla formularza trasy pochodzą z roboczej listy miast (nie z katalogu nazw).
    private IReadOnlyList<string> SuggestCityNames(string query)
    {
        var trimmed = query?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return [];
        }

        return Cities
            .Select(c => c.Name)
            .Where(n => n.Contains(trimmed, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(n => n.StartsWith(trimmed, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
            .ThenBy(n => n, StringComparer.CurrentCultureIgnoreCase)
            .Take(MaxNameSuggestions)
            .ToArray();
    }

    private City? ResolveCityByName(string name)
    {
        var trimmed = name?.Trim();
        return string.IsNullOrEmpty(trimmed)
            ? null
            : Cities.FirstOrDefault(c => string.Equals(c.Name, trimmed, StringComparison.CurrentCultureIgnoreCase));
    }

    private void SetRouteCityFromText(string name)
    {
        _suppressRouteFromSuggestions = true;
        RouteCityFromText = name;
        _suppressRouteFromSuggestions = false;
        RouteCityFromSuggestions.Clear();
        OnPropertyChanged(nameof(HasRouteCityFromSuggestions));
    }

    private void SetRouteCityToText(string name)
    {
        _suppressRouteToSuggestions = true;
        RouteCityToText = name;
        _suppressRouteToSuggestions = false;
        RouteCityToSuggestions.Clear();
        OnPropertyChanged(nameof(HasRouteCityToSuggestions));
    }

    private void RefreshCities()
    {
        Cities.Clear();
        foreach (var city in editor.Cities)
        {
            Cities.Add(city);
        }

        RefreshRoutes();
    }

    private void ClearForm()
    {
        EditingCity = null;
        _suppressSuggestions = true;
        CityName = string.Empty;
        _suppressSuggestions = false;
        ClearSuggestions();
        PickedPosition = null;
    }

    private void ClearSuggestions()
    {
        NameSuggestions.Clear();
        OnPropertyChanged(nameof(HasSuggestions));
    }

    private void RefreshRoutes()
    {
        Routes.Clear();
        foreach (var route in editor.Routes)
        {
            Routes.Add(new DeveloperRouteRow(route, GetCityName(route.CityFromId), GetCityName(route.CityToId)));
        }

        OnOverlayChanged();
    }

    private string GetCityName(string cityId) => Cities.FirstOrDefault(c => c.Id == cityId)?.Name ?? cityId;

    private void ClearRouteForm()
    {
        EditingRoute = null;
        SetRouteCityFromText(string.Empty);
        SetRouteCityToText(string.Empty);
        PendingWagons.Clear();
        _pendingWagonFirstPoint = null;
        _editingWagonIndex = null;
        OnPropertyChanged(nameof(WagonPickHint));
        SaveRouteCommand.NotifyCanExecuteChanged();
    }

    private void OnOverlayChanged() => OverlayChanged?.Invoke(this, EventArgs.Empty);
}

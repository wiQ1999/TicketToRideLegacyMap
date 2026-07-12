namespace Aplication.Pages;

public partial class MapPage : ContentPage
{
    private const int MaxSearchSuggestions = 6;

    private readonly IMapDataProvider _mapDataProvider;
    private readonly IMapInteractionState _interactionState;
    private readonly IErrorHandler _errorHandler;
    private bool _loaded;
    private MapData? _map;
    private MapBoardView? _board;
    private bool _suppressSearchTextChanged;

    public MapPage(
        IMapDataProvider mapDataProvider,
        IMapInteractionState interactionState,
        IErrorHandler errorHandler)
    {
        InitializeComponent();
        _mapDataProvider = mapDataProvider;
        _interactionState = interactionState;
        _errorHandler = errorHandler;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        LoadMapAsync().FireAndForgetSafeAsync(_errorHandler);
    }

    private async Task LoadMapAsync()
    {
        _map = await _mapDataProvider.GetMapDataAsync();

        _board = new MapBoardView(_map, _interactionState);
        RootLayout.Insert(0, _board); // pod licznikami, wyszukiwarką, guzikami zoomu i wskaźnikiem
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        _interactionState.Changed += OnInteractionStateChanged;
        UpdateCounters();
    }

    private void OnInteractionStateChanged(object? sender, EventArgs e) => UpdateCounters();

    private void OnZoomInTapped(object? sender, TappedEventArgs e) => _board?.ZoomIn();

    private void OnZoomOutTapped(object? sender, TappedEventArgs e) => _board?.ZoomOut();

    private void OnSearchButtonTapped(object? sender, TappedEventArgs e) => SetSearchExpanded(true);

    private void OnCitySearchUnfocused(object? sender, FocusEventArgs e)
    {
        // Zwiń po utracie focusu — z opóźnieniem, bo tap w podpowiedź najpierw odbiera focus polu.
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
        {
            if (SearchExpanded.IsVisible && !CitySearchEntry.IsFocused)
            {
                SetSearchExpanded(false);
            }
        });
    }

    private void SetSearchExpanded(bool expanded)
    {
        SearchExpanded.IsVisible = expanded;
        CollapsedSearchButton.IsVisible = !expanded;

        if (expanded)
        {
            Dispatcher.Dispatch(() => CitySearchEntry.Focus());
            return;
        }

        _suppressSearchTextChanged = true;
        CitySearchEntry.Text = string.Empty;
        _suppressSearchTextChanged = false;
        ClearSearchButton.IsVisible = false;
        CitySearchResults.ItemsSource = null;
        CitySearchResultsPanel.IsVisible = false;
    }

    private void OnCitySearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppressSearchTextChanged || _map is null)
        {
            return;
        }

        ClearSearchButton.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);

        var query = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            CitySearchResults.ItemsSource = null;
            CitySearchResultsPanel.IsVisible = false;
            return;
        }

        var matches = _map.Cities
            .Where(city => city.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(city => city.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(city => city.Name, StringComparer.OrdinalIgnoreCase)
            .Take(MaxSearchSuggestions)
            .ToList();

        CitySearchResults.ItemsSource = matches;
        CitySearchResultsPanel.IsVisible = matches.Count > 0;
    }

    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
    {
        CitySearchEntry.Text = string.Empty;
        CitySearchEntry.Focus();
    }

    private void OnCitySearchResultSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not City city || _board is null)
        {
            return;
        }

        CitySearchResults.SelectedItem = null;

        _board.CenterOnCity(city);

        if (!_interactionState.IsCityMarked(city.Id))
        {
            _interactionState.ToggleCity(city.Id);
        }

        SetSearchExpanded(false);
    }

    private void UpdateCounters()
    {
        if (_map is null)
        {
            return;
        }

        var selectedWagons = 0;
        var doneWagons = 0;
        foreach (var route in _map.Routes)
        {
            switch (_interactionState.GetRouteState(route.Id))
            {
                case RouteState.Selected:
                    selectedWagons += route.WagonCount;
                    break;
                case RouteState.Done:
                    doneWagons += route.WagonCount;
                    break;
            }
        }

        SelectedCountLabel.Text = selectedWagons.ToString();
        DoneCountLabel.Text = doneWagons.ToString();
    }
}

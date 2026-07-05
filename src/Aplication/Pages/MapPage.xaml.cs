namespace Aplication.Pages;

public partial class MapPage : ContentPage
{
    private readonly IMapDataProvider _mapDataProvider;
    private readonly IMapInteractionState _interactionState;
    private readonly IErrorHandler _errorHandler;
    private bool _loaded;
    private MapData? _map;

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

        var board = new MapBoardView(_map, _interactionState);
        RootLayout.Insert(0, board); // pod legendą, licznikami i wskaźnikiem
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        _interactionState.Changed += OnInteractionStateChanged;
        UpdateCounters();
    }

    private void OnInteractionStateChanged(object? sender, EventArgs e) => UpdateCounters();

    private void OnSettingsClicked(object? sender, EventArgs e) =>
        Shell.Current.GoToAsync("settings").FireAndForgetSafeAsync(_errorHandler);

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

        CountersLabel.Text = $"Zaznaczone / wykonane: {selectedWagons} / {doneWagons}";
    }
}

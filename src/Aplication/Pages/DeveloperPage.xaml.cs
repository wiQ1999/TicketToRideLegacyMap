namespace Aplication.Pages;

public partial class DeveloperPage : ContentPage
{
    private readonly DeveloperPageModel _pageModel;
    private readonly IErrorHandler _errorHandler;
    private MapBoardView? _board;

    public DeveloperPage(DeveloperPageModel pageModel, IErrorHandler errorHandler)
    {
        InitializeComponent();
        _pageModel = pageModel;
        _errorHandler = errorHandler;
        BindingContext = pageModel;
        _pageModel.OverlayChanged += OnOverlayChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeAsync().FireAndForgetSafeAsync(_errorHandler);
    }

    private async Task InitializeAsync()
    {
        await _pageModel.InitializeAsync();

        if (_board is null && _pageModel.Map is { } map)
        {
            _board = new MapBoardView(map, null);
            _board.MapPointPicked += OnMapPointPicked;
            RootGrid.Children.Insert(0, _board);
            Grid.SetColumn(_board, 0);

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        RefreshOverlay();
    }

    private void OnMapPointPicked(object? sender, MapPoint point) => _pageModel.HandleMapTap(point);

    private void OnOverlayChanged(object? sender, EventArgs e) => RefreshOverlay();

    private void RefreshOverlay()
    {
        if (_board is null)
        {
            return;
        }

        var markers = _pageModel.Cities.Select(c => c.Position).ToList();
        _board.SetDeveloperOverlay(
            markers,
            _pageModel.OverlayPendingPoint,
            _pageModel.IsOverlayPendingPointWagonCorner,
            _pageModel.PendingWagons);
    }

    private void OnNameSuggestionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not string name)
        {
            return;
        }

        ((CollectionView)sender!).SelectedItem = null;
        _pageModel.SelectSuggestionCommand.Execute(name);
    }

    private void OnRouteCityFromSuggestionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not string name)
        {
            return;
        }

        ((CollectionView)sender!).SelectedItem = null;
        _pageModel.SelectRouteCityFromSuggestionCommand.Execute(name);
    }

    private void OnRouteCityToSuggestionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not string name)
        {
            return;
        }

        ((CollectionView)sender!).SelectedItem = null;
        _pageModel.SelectRouteCityToSuggestionCommand.Execute(name);
    }
}

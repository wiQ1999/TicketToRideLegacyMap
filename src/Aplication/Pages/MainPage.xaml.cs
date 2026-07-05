namespace Aplication.Pages;

public partial class MainPage : ContentPage
{
    private readonly IMapDataProvider _mapDataProvider;
    private readonly IErrorHandler _errorHandler;
    private bool _loaded;

    public MainPage(
        IMapDataProvider mapDataProvider,
        IErrorHandler errorHandler)
    {
        InitializeComponent();
        _mapDataProvider = mapDataProvider;
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
        var map = await _mapDataProvider.GetMapDataAsync();

        var board = new MapBoardView(map);
        RootLayout.Insert(0, board); // pod legendą i wskaźnikiem
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
    }
}

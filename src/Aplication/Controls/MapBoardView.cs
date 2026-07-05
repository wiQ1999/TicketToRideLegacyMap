using Aplication.Rendering;

namespace Aplication.Controls;

/// <summary>
/// Interaktywna plansza: jeden <see cref="GraphicsView"/> rysujący całą mapę wektorowo, z gestami
/// zoomu (pinch), przesuwania (pan) oraz dotyku (tap). W trybie mapy (przekazany stan interakcji)
/// tap przechodzi przez <see cref="MapHitTester"/> do toggle miasta lub cyklu trasy. W trybie
/// deweloperskim (stan = <c>null</c>) tap zgłasza wskazane położenie przez <see cref="MapPointPicked"/>,
/// a plansza służy jako podkład z nakładką roboczą.
/// </summary>
public sealed class MapBoardView : ContentView
{
    private readonly MapViewport _viewport;
    private readonly MapDrawable _drawable;
    private readonly MapHitTester _hitTester;
    private readonly IMapInteractionState? _state;
    private readonly GraphicsView _graphicsView;

    private double _pinchStartScale = 1.0;
    private double _lastPanX;
    private double _lastPanY;

    /// <summary>Zgłaszane po dotknięciu planszy w trybie deweloperskim — z punktem w przestrzeni mapy.</summary>
    public event EventHandler<MapPoint>? MapPointPicked;

    public MapBoardView(MapData map, IMapInteractionState? state)
    {
        _state = state;
        _viewport = new MapViewport(map.CanvasSize.Width, map.CanvasSize.Height);
        _drawable = new MapDrawable(map, _viewport, state);
        _hitTester = new MapHitTester(map);

        _graphicsView = new GraphicsView
        {
            Drawable = _drawable,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        AddGestures();
        Content = _graphicsView;

        _graphicsView.SizeChanged += OnSizeChanged;
        if (_state is not null)
        {
            _state.Changed += OnStateChanged;
        }

        LoadBackgroundAsync().FireAndForgetSafeAsync();
    }

    /// <summary>Ustawia nakładkę roboczą trybu deweloperskiego (znaczniki miast + wskazany punkt) i przerysowuje.</summary>
    public void SetDeveloperOverlay(IReadOnlyList<MapPoint> markers, MapPoint? pendingPoint)
    {
        _drawable.DeveloperMarkers = markers;
        _drawable.DeveloperPendingPoint = pendingPoint;
        _graphicsView.Invalidate();
    }

    private void AddGestures()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        _graphicsView.GestureRecognizers.Add(tap);

        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnPinchUpdated;
        _graphicsView.GestureRecognizers.Add(pinch);

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        _graphicsView.GestureRecognizers.Add(pan);
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (_graphicsView.Width <= 0 || _graphicsView.Height <= 0)
        {
            return;
        }

        // Re-kalibracja widoku „z lotu ptaka" przy każdej zmianie rozmiaru.
        _viewport.ResetToFit(_graphicsView.Width, _graphicsView.Height);
        _graphicsView.Invalidate();
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        var position = e.GetPosition(_graphicsView);
        if (position is not { } p)
        {
            return;
        }

        // Tryb deweloperski: tap wskazuje surowe położenie na mapie, bez oznaczania miast/tras.
        if (_state is not { } state)
        {
            MapPointPicked?.Invoke(this, _viewport.ScreenToMap(p.X, p.Y));
            return;
        }

        var hit = _hitTester.HitTest(new PointF((float)p.X, (float)p.Y), _viewport);
        switch (hit.Kind)
        {
            case MapHitKind.City:
                state.ToggleCity(hit.Id);
                break;
            case MapHitKind.Route:
                state.CycleRoute(hit.Id);
                break;
        }
    }

    private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _pinchStartScale = _viewport.Scale;
                break;
            case GestureStatus.Running:
                var pivot = new PointF(
                    (float)(e.ScaleOrigin.X * _graphicsView.Width),
                    (float)(e.ScaleOrigin.Y * _graphicsView.Height));
                _viewport.ZoomTo(_pinchStartScale * e.Scale, pivot);
                _graphicsView.Invalidate();
                break;
        }
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _lastPanX = 0;
                _lastPanY = 0;
                break;
            case GestureStatus.Running:
                _viewport.PanBy(e.TotalX - _lastPanX, e.TotalY - _lastPanY);
                _lastPanX = e.TotalX;
                _lastPanY = e.TotalY;
                _graphicsView.Invalidate();
                break;
        }
    }

    private void OnStateChanged(object? sender, EventArgs e) => _graphicsView.Invalidate();

    /// <summary>Programowo przybliża i centruje widok na wskazanym mieście (wynik wyszukiwania).</summary>
    public void CenterOnCity(City city)
    {
        _viewport.CenterOn(city.Position);
        _graphicsView.Invalidate();
    }

    private async Task LoadBackgroundAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("map_background.png")
                .ConfigureAwait(false);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory).ConfigureAwait(false);
            memory.Position = 0;
            _drawable.Background = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(memory);
            _graphicsView.Invalidate();
        }
        catch
        {
            // Brak podkładu nie jest błędem krytycznym — tło rysowane jest wtedy kolorem.
        }
    }
}

using Aplication.Rendering;

namespace Aplication.Controls;

/// <summary>
/// Interaktywna plansza: jeden <see cref="GraphicsView"/> rysujący całą mapę wektorowo, z gestami
/// zoomu (pinch), przesuwania (pan) oraz dotyku (tap). W trybie mapy (przekazany stan interakcji)
/// tap przechodzi przez <see cref="MapHitTester"/> do toggle miasta lub cyklu trasy. W trybie
/// deweloperskim (stan = <c>null</c>) tap zgłasza wskazane położenie przez <see cref="MapPointPicked"/>,
/// a plansza służy jako podkład z nakładką roboczą.
/// </summary>
public sealed partial class MapBoardView : ContentView
{
    private const double ZoomStep = 1.3;

    private readonly MapViewport _viewport;
    private readonly MapDrawable _drawable;
    private readonly MapHitTester _hitTester;
    private readonly IMapInteractionState? _state;
    private readonly GraphicsView _graphicsView;

#if !ANDROID
    private double _pinchStartScale = 1.0;
    private double _lastPanX;
    private double _lastPanY;
#endif

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
        _state?.Changed += OnStateChanged;

        LoadBackgroundAsync().FireAndForgetSafeAsync();
        LoadCityStarAsync().FireAndForgetSafeAsync();
        LoadWagonLockAsync().FireAndForgetSafeAsync();
    }

    /// <summary>
    /// Ustawia nakładkę roboczą trybu deweloperskiego (znaczniki miast, wskazany punkt — miasta lub
    /// róg wagonika, oraz wagoniki roboczej trasy) i przerysowuje.
    /// </summary>
    public void SetDeveloperOverlay(
        IReadOnlyList<MapPoint> markers,
        MapPoint? pendingPoint,
        bool isPendingPointWagonCorner,
        IReadOnlyList<WagonRectangle> wagons)
    {
        _drawable.DeveloperMarkers = markers;
        _drawable.DeveloperPendingPoint = pendingPoint;
        _drawable.DeveloperPendingPointIsWagonCorner = isPendingPointWagonCorner;
        _drawable.DeveloperWagons = wagons;
        _graphicsView.Invalidate();
    }

    private void AddGestures()
    {
#if ANDROID
        // Multi-touch pinch/pan na GraphicsView jest na Androidzie zawodny (ograniczenie MAUI) —
        // wszystkie gesty (tap/pan/pinch) obsługuje natywny handler dotyku (partial dla Androida).
        HookPlatformGestures();
#else
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        _graphicsView.GestureRecognizers.Add(tap);

        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnPinchUpdated;
        _graphicsView.GestureRecognizers.Add(pinch);

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        _graphicsView.GestureRecognizers.Add(pan);
#endif
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

    /// <summary>
    /// Wykonuje dotyk w punkcie ekranu (DIP). W trybie mapy przechodzi przez hit-testing do toggle
    /// miasta / cyklu trasy; w trybie deweloperskim zgłasza surowe położenie przez <see cref="MapPointPicked"/>.
    /// </summary>
    private void PerformTap(double x, double y)
    {
        if (_state is not { } state)
        {
            MapPointPicked?.Invoke(this, _viewport.ScreenToMap(x, y));
            return;
        }

        var hit = _hitTester.HitTest(new PointF((float)x, (float)y), _viewport);
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

    private void PanByScreen(double dxScreen, double dyScreen)
    {
        _viewport.PanBy(dxScreen, dyScreen);
        _graphicsView.Invalidate();
    }

    private void ScaleBy(double factor, float pivotX, float pivotY)
    {
        _viewport.ZoomTo(_viewport.Scale * factor, new PointF(pivotX, pivotY));
        _graphicsView.Invalidate();
    }

#if !ANDROID
    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (e.GetPosition(_graphicsView) is { } p)
        {
            PerformTap(p.X, p.Y);
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
#endif

    private void OnStateChanged(object? sender, EventArgs e) => _graphicsView.Invalidate();

    /// <summary>Programowo przybliża i centruje widok na wskazanym mieście (wynik wyszukiwania).</summary>
    public void CenterOnCity(City city)
    {
        _viewport.CenterOn(city.Position);
        _graphicsView.Invalidate();
    }

    /// <summary>Programowo przybliża widok wokół środka kadru (przycisk „+").</summary>
    public void ZoomIn() => ZoomAroundCenter(ZoomStep);

    /// <summary>Programowo oddala widok wokół środka kadru (przycisk „−").</summary>
    public void ZoomOut() => ZoomAroundCenter(1.0 / ZoomStep);

    private void ZoomAroundCenter(double factor)
    {
        if (_graphicsView.Width <= 0 || _graphicsView.Height <= 0)
        {
            return;
        }

        var center = new PointF((float)(_graphicsView.Width / 2.0), (float)(_graphicsView.Height / 2.0));
        _viewport.ZoomTo(_viewport.Scale * factor, center);
        _graphicsView.Invalidate();
    }

    private async Task LoadBackgroundAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("map_background.jpg")
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

    private async Task LoadCityStarAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("city-star.png")
                .ConfigureAwait(false);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory).ConfigureAwait(false);
            memory.Position = 0;
            _drawable.CityStar = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(memory);
            _graphicsView.Invalidate();
        }
        catch
        {
            // Brak ikony gwiazdy nie jest błędem krytycznym — znacznik rysowany jest wtedy bez niej.
        }
    }

    private async Task LoadWagonLockAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("wagon-lock.png")
                .ConfigureAwait(false);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory).ConfigureAwait(false);
            memory.Position = 0;
            _drawable.WagonLock = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(memory);
            _graphicsView.Invalidate();
        }
        catch
        {
            // Brak ikony kłódki nie jest błędem krytycznym — wagonik wykonanej trasy rysowany jest wtedy bez niej.
        }
    }
}

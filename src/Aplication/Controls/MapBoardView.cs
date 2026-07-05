using Aplication.Rendering;

namespace Aplication.Controls;

/// <summary>
/// Plansza mapy: jeden <see cref="GraphicsView"/> rysujący całą mapę wektorowo, z gestami zoomu
/// (pinch) i przesuwania (pan) modyfikującymi wspólny <see cref="MapViewport"/>. Na tym etapie
/// widok służy wyłącznie do wyświetlania — bez oznaczania miast i tras.
/// </summary>
public sealed class MapBoardView : ContentView
{
    private readonly MapViewport _viewport;
    private readonly MapDrawable _drawable;
    private readonly GraphicsView _graphicsView;

    private double _pinchStartScale = 1.0;
    private double _lastPanX;
    private double _lastPanY;

    public MapBoardView(MapData map)
    {
        _viewport = new MapViewport(map.CanvasSize.Width, map.CanvasSize.Height);
        _drawable = new MapDrawable(map, _viewport);

        _graphicsView = new GraphicsView
        {
            Drawable = _drawable,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        AddGestures();
        Content = _graphicsView;

        _graphicsView.SizeChanged += OnSizeChanged;

        LoadBackgroundAsync().FireAndForgetSafeAsync();
    }

    private void AddGestures()
    {
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

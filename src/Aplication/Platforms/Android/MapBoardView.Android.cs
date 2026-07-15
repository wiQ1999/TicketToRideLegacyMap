using Android.Views;
using AView = Android.Views.View;

namespace Aplication.Controls;

public sealed partial class MapBoardView
{
    private ScaleGestureDetector? _scaleDetector;
    private GestureDetector? _gestureDetector;
    private float _density = 1f;

    // Gesty na Androidzie: natywne detektory dotyku zamiast (zawodnych na GraphicsView)
    // rozpoznawaczy MAUI. ScaleGestureDetector daje pinch + przesuwanie środka gestu (dwa palce),
    // GestureDetector — pan jednym palcem oraz tap.
    private void HookPlatformGestures()
    {
        _graphicsView.HandlerChanged += (_, _) => AttachTouchHandling();
    }

    private void AttachTouchHandling()
    {
        if (_graphicsView.Handler?.PlatformView is not AView view || view.Context is not { } context)
        {
            return;
        }

        _density = context.Resources?.DisplayMetrics?.Density ?? 1f;
        _scaleDetector = new ScaleGestureDetector(context, new PinchListener(this));
        _gestureDetector = new GestureDetector(context, new PanTapListener(this))
        {
            IsLongpressEnabled = false
        };

        view.Touch -= OnPlatformTouch;
        view.Touch += OnPlatformTouch;
    }

    private void OnPlatformTouch(object? sender, AView.TouchEventArgs e)
    {
        if (e.Event is { } motion)
        {
            _scaleDetector?.OnTouchEvent(motion);
            _gestureDetector?.OnTouchEvent(motion);
        }

        e.Handled = true;
    }

    private float ToDip(float px) => px / _density;

    private sealed class PinchListener(MapBoardView owner) : ScaleGestureDetector.SimpleOnScaleGestureListener
    {
        private float _lastFocusX;
        private float _lastFocusY;

        public override bool OnScaleBegin(ScaleGestureDetector detector)
        {
            _lastFocusX = detector.FocusX;
            _lastFocusY = detector.FocusY;
            return true;
        }

        public override bool OnScale(ScaleGestureDetector detector)
        {
            // Dwupalcowe przesuwanie: pan o ruch środka gestu; skalowanie wokół tego środka.
            owner.PanByScreen(owner.ToDip(detector.FocusX - _lastFocusX), owner.ToDip(detector.FocusY - _lastFocusY));
            _lastFocusX = detector.FocusX;
            _lastFocusY = detector.FocusY;

            owner.ScaleBy(detector.ScaleFactor, owner.ToDip(detector.FocusX), owner.ToDip(detector.FocusY));
            return true;
        }
    }

    private sealed class PanTapListener(MapBoardView owner) : GestureDetector.SimpleOnGestureListener
    {
        public override bool OnSingleTapUp(MotionEvent e)
        {
            owner.PerformTap(owner.ToDip(e.GetX()), owner.ToDip(e.GetY()));
            return true;
        }

        public override bool OnScroll(MotionEvent? e1, MotionEvent e2, float distanceX, float distanceY)
        {
            // Jednopalcowy pan; przesuwanie dwoma palcami obsługuje ScaleGestureDetector (środek gestu).
            if (e2.PointerCount != 1)
            {
                return false;
            }

            owner.PanByScreen(owner.ToDip(-distanceX), owner.ToDip(-distanceY));
            return true;
        }
    }
}

using Entities;
using RenderEngine;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Viewing
{
    public class Viewport: ContentControl
    {
        public static readonly DependencyProperty VisualsProperty = 
            DependencyProperty.Register("Visuals", typeof(VisualsCollection), typeof(Viewport), new PropertyMetadata(new VisualsCollection(), OnVisualsChanged));
        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register("Camera", typeof(Camera), typeof(Viewport), new PropertyMetadata(new Camera(new Space()),  OnCameraChanged));
        public static readonly DependencyProperty InteractorLeftProperty =
            DependencyProperty.Register("InteractorLeft", typeof(IMouseInteractor), typeof(Viewport), new PropertyMetadata(null));
        public static readonly DependencyProperty InteractorRightProperty =
            DependencyProperty.Register("InteractorRight", typeof(IMouseInteractor), typeof(Viewport), new PropertyMetadata(null));

        public ViewportRenderer ViewportRenderer { get; private set; }

        private WriteableBitmap _bitmap;
        private CollectionObserver<IVisual> _visualsObserver;
        private Camera _camera;

        public Viewport()
        {
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            ViewportRenderer = new ViewportRenderer();
           
            InteractorRight = new ZoomInteractor();

            Unloaded += OnUnload;
            Loaded += OnLoad;

            Dispatcher.ShutdownStarted += ShutdownStarted;
        }

        private void ShutdownStarted(object sender, EventArgs e)
        {
            OnUnload(sender, new RoutedEventArgs());
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            _visualsObserver?.Dispose();
            ViewportRenderer?.Dispose();
            ViewportRenderer = null;

            Camera.PropertyChanged -= OnCameraChanged;
            Camera.Space.GetRoot().Changed -= OnCameraChanged;
        }

        public VisualsCollection Visuals
        {
            get => (VisualsCollection)GetValue(VisualsProperty);
            set => SetValue(VisualsProperty, value);
        }

        public Camera Camera
        {
            get => (Camera)GetValue(CameraProperty);
            set => SetValue(CameraProperty, value);
        }

        public IMouseInteractor InteractorLeft
        {
            get => (IMouseInteractor)GetValue(InteractorLeftProperty);
            set => SetValue(InteractorLeftProperty, value);
        }

        public IMouseInteractor InteractorRight
        {
            get => (IMouseInteractor)GetValue(InteractorRightProperty);
            set => SetValue(InteractorRightProperty, value);
        }

        private void OnVisualsChanged(VisualsCollection visuals)
        {
            _visualsObserver?.Dispose();
            _visualsObserver = new CollectionObserver<IVisual>(visuals, OnVisualAdded, OnVisualRemoved);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && InteractorLeft != null)
            {
                CaptureMouse();
                InteractorLeft?.MouseDown(e.GetPosition(this), this);
            }
            else if (e.RightButton == MouseButtonState.Pressed && InteractorRight != null)
            {
                CaptureMouse();
                InteractorRight?.MouseDown(e.GetPosition(this), this);
            }
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool handled = false;
            if (e.LeftButton == MouseButtonState.Pressed && InteractorLeft != null)
            {
                CaptureMouse();
                handled = InteractorLeft.MouseMove(e.GetPosition(this), this);
            }
            else if (e.RightButton == MouseButtonState.Pressed && InteractorRight != null)
            {
                handled = InteractorRight.MouseMove(e.GetPosition(this), this);
            }
            else if (InteractorLeft != null)
            {
                handled = InteractorLeft.MouseMove(e.GetPosition(this), this);
            }

            if (handled)
            {
                InvalidateVisual();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            InteractorRight?.MouseUp(e.GetPosition(this), this);
            InteractorLeft?.MouseUp(e.GetPosition(this), this);
            InvalidateVisual();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!IsLoaded) { return; }
            Camera.Zoom = Math.Max(1, Camera.Zoom + e.Delta / -10);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ActualWidth == 0 || ActualHeight == 0 || ViewportRenderer == null || _camera == null) { return; }

            ViewportRenderer.SetZoom(_camera.Zoom);
            ViewportRenderer.SetCameraTransformation(_camera.GetTransformation());

            foreach (var visual in Visuals)
            {
                visual.PreRender(ViewportRenderer);
            }

            int width = (int)ActualWidth;
            int height = (int)ActualHeight;
            ViewportRenderer.SetSize(width, height);

            if (_bitmap == null || _bitmap.Width != width || _bitmap.Height != height)
            {
                _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            }
            _bitmap.Lock();
            ViewportRenderer.Render(_bitmap.BackBuffer);
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            _bitmap.Unlock();

            drawingContext.DrawImage(_bitmap, new Rect(new Point(0, 0), new Size(width, height)));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            InvalidateVisual();
        }

        private void OnVisualAdded(IVisual visual)
        {
            visual.Invalidated += OnVisualInvalidated;
            visual.AddTo(ViewportRenderer);
            InvalidateVisual();
        }

        private void OnVisualInvalidated()
        {
            InvalidateVisual();
        }

        private void OnVisualRemoved(IVisual visual)
        {
            visual.Invalidated -= OnVisualInvalidated;
            visual.RemoveFrom(ViewportRenderer);
            InvalidateVisual();
        }

        private void OnCameraChanged(Camera camera)
        {
            _camera = camera;
            camera.Space.GetRoot().Changed += OnCameraChanged;
            OnCameraChanged();
            camera.PropertyChanged += OnCameraChanged;
        }

        private void OnCameraChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ViewportRenderer == null) { return; }
            InvalidateVisual();
        }

        private void OnCameraChanged()
        {
            if (ViewportRenderer == null) { return; }
            InvalidateVisual();
        }

        private static void OnVisualsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Viewport)d).OnVisualsChanged((VisualsCollection)e.NewValue);
        }

        private static void OnCameraChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Viewport)d).OnCameraChanged((Camera)e.NewValue);
        }
    }
}

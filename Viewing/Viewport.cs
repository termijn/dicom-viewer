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
            DependencyProperty.Register("Camera", typeof(Camera), typeof(Viewport), new PropertyMetadata(new Camera(),  OnCameraChanged));
        public static readonly DependencyProperty InteractorLeftProperty =
            DependencyProperty.Register("InteractorLeft", typeof(IMouseInteractor), typeof(Viewport), new PropertyMetadata(null));
        public static readonly DependencyProperty InteractorRightProperty =
            DependencyProperty.Register("InteractorRight", typeof(IMouseInteractor), typeof(Viewport), new PropertyMetadata(null));

        public ViewportRenderer ViewportRenderer { get; }

        private WriteableBitmap _bitmap;
        private CollectionObserver<IVisual> _visualsObserver;
        private PropertySubscription _cameraSubscription;
        private Camera _camera;

        public Viewport()
        {
            ViewportRenderer = new ViewportRenderer();
            
            InteractorLeft = new RotateCameraInteractor(Camera);
            InteractorRight = new ZoomInteractor(Camera);

            Dispatcher.ShutdownStarted += OnShutdownStarted;
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
            if (_visualsObserver != null)
            {
                _visualsObserver.Dispose();
            }
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
            Camera.Zoom = Math.Max(10, Camera.Zoom + e.Delta / -10);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ActualWidth == 0 || ActualHeight == 0) return;

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
            visual.AddTo(ViewportRenderer);
            InvalidateVisual();
        }

        private void OnVisualRemoved(IVisual visual)
        {
            visual.RemoveFrom(ViewportRenderer);
            InvalidateVisual();
        }

        private void OnCameraChanged(Camera camera)
        {
            _camera = camera;
            _cameraSubscription = new PropertySubscription(camera, OnCameraTransformationChanged);
            OnCameraTransformationChanged(this, new PropertyChangedEventArgs(string.Empty));
        }

        private void OnCameraTransformationChanged(object sender, PropertyChangedEventArgs e)
        {
            ViewportRenderer.SetZoom(_camera.Zoom);
            ViewportRenderer.SetCameraTransformation(_camera.TransformationToWorld * _camera.ViewportPan);
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

        private void OnShutdownStarted(object sender, EventArgs e)
        {
            ViewportRenderer.Dispose();
        }
    }
}

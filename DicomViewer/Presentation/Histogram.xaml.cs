using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DicomViewer.Presentation
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : UserControl
    {
        public static readonly DependencyProperty WindowLevelProperty =
            DependencyProperty.Register("WindowLevel", typeof(double), typeof(Histogram), new FrameworkPropertyMetadata(1000d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, WindowLevelChanged));
        public static readonly DependencyProperty WindowWidthProperty =
            DependencyProperty.Register("WindowWidth", typeof(double), typeof(Histogram), new FrameworkPropertyMetadata(1000d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, WindowLevelChanged));

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(double), typeof(Histogram), new FrameworkPropertyMetadata(-2000d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, WindowLevelChanged));
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(double), typeof(Histogram), new FrameworkPropertyMetadata(2000d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, WindowLevelChanged));

        private bool isDraggingCenterHandle;
        private bool isDraggingWidthHandle;
        private double startLevel;
        private double startWidth;
        private Point startPoint;        
        private bool isMouseOverCenterHandle;
        private bool isMouseOverWidthHandle;

        public Histogram()
        {
            InitializeComponent();
        }

        public double WindowLevel
        {
            get { return (double)GetValue(WindowLevelProperty); }
            set { SetValue(WindowLevelProperty, value); }
        }

        public double WindowWidth
        {
            get { return (double)GetValue(WindowWidthProperty); }
            set { SetValue(WindowWidthProperty, value); }
        }

        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ActualWidth == 0 || ActualHeight == 0) { return; }
            double rangeHU = Max - Min;

            double windowMin = WindowLevel - WindowWidth / 2;
            double windowMax = WindowLevel + WindowWidth / 2;
            
            var pen = new Pen(Brushes.White, 2);
            drawingContext.DrawLine(pen, new Point(0, ActualHeight), new Point(HUToX(windowMin), ActualHeight ));
            drawingContext.DrawLine(pen, new Point(HUToX(windowMin), ActualHeight), new Point(HUToX(windowMax), 0));
            drawingContext.DrawLine(pen, new Point(HUToX(windowMax), 0), new Point(ActualWidth, 0));

            drawingContext.DrawEllipse(isMouseOverCenterHandle || isDraggingCenterHandle ? Brushes.White : Brushes.Transparent, pen, new Point(HUToX(WindowLevel), ActualHeight / 2), 7, 7);
            drawingContext.DrawEllipse(isMouseOverWidthHandle || isDraggingWidthHandle ? Brushes.White : Brushes.Transparent, pen, new Point(HUToX(windowMax), 0), 7, 7);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            CheckMouseOverCenterHandle(e);
            if (isMouseOverCenterHandle)
            {
                isDraggingCenterHandle = true;
                startLevel = WindowLevel;
                startPoint = e.GetPosition(this);
                CaptureMouse();
            }
            if (isMouseOverWidthHandle)
            {
                isDraggingWidthHandle = true;
                startWidth = WindowWidth;
                startPoint = e.GetPosition(this);
                CaptureMouse();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            CheckMouseOverCenterHandle(e);
            ReleaseMouseCapture();
            isDraggingCenterHandle = false;
            isDraggingWidthHandle = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            CheckMouseOverCenterHandle(e);

            if (isDraggingCenterHandle)
            {
                var delta = XToHU(startPoint.X) - XToHU(e.GetPosition(this).X);
                WindowLevel = Math.Min(Math.Max(Min, startLevel - delta), Max);
            }
            else if (isDraggingWidthHandle)
            {
                var delta = XToHU(startPoint.X) - XToHU(e.GetPosition(this).X);
                WindowWidth = Math.Min(Math.Max(0.1, startWidth - delta * 2), Max - Min);
            }         
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            CheckMouseOverCenterHandle(e);
        }

        private void CheckMouseOverCenterHandle(MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            var centerHandlePosition = new Point(HUToX(WindowLevel), ActualHeight / 2);
            var widthHandlePosition = new Point(HUToX(WindowLevel + WindowWidth / 2), 0);
            isMouseOverCenterHandle = (centerHandlePosition - position).Length < 7;
            isMouseOverWidthHandle = (widthHandlePosition - position).Length < 7;
            InvalidateVisual();
        }

        private double HUToX(double HU)
        {
            double rangeHU = Max - Min;
            return ((HU - Min) / rangeHU) * ActualWidth;
        }

        private double XToHU(double x)
        {
            double rangeHU = Max - Min;

            double factor = x / ActualWidth;
            double HU = rangeHU * factor - Min;
            return HU;
        }

        public void UpdateWindowingLines()
        {
            InvalidateVisual();
        }

        private static void WindowLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Histogram)d).UpdateWindowingLines();
        }
    }
}

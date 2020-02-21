using System;
using System.Windows;

namespace Viewing
{
    public class ZoomInteractor : IMouseInteractor
    {
        private readonly Camera camera;
        Point _startPosition;
        double _startFactor = 100;
        bool isDragging;

        public ZoomInteractor(Camera camera)
        {
            this.camera = camera;
        }

        public void MouseDown(Point position, Viewport viewport)
        {
            _startPosition = position;
            _startFactor = camera.Zoom;
            isDragging = true;
        }

        public bool MouseMove(Point position, Viewport viewport)
        {
            if (!isDragging) { return false; }
            double deltaY = (position - _startPosition).Y;
            camera.Zoom = _startFactor * Math.Pow(2, 0.01 * (deltaY));
            return true;
        }

        public void MouseUp(Point position, Viewport viewport)
        {
            isDragging = false;
        }
    }
}

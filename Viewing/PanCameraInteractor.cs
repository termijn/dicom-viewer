using System.Windows;
using Entities;

namespace Viewing
{
    public class PanCameraInteractor : IMouseInteractor
    {
        private bool _isMouseDown;
        private Matrix _initialTransform;
        private Point _initialPosition;

        public void MouseDown(Point position, Viewport viewport)
        {
            _isMouseDown = true;
            _initialTransform = viewport.Camera.ViewportPan;
            _initialPosition = position;
        }

        public bool MouseMove(Point position, Viewport viewport)
        {
            if (!_isMouseDown)
            {
                return false;
            }

            var delta = (position - _initialPosition);
            var viewportHeightInMm = viewport.Camera.Zoom * 2;
            var mmPerPixel = viewportHeightInMm / viewport.ActualHeight;
            var deltaInMm = delta * mmPerPixel;
            viewport.Camera.ViewportPan = Matrix.Translation(new Vector3(deltaInMm.X, -deltaInMm.Y, 0)) * _initialTransform;
            return true;
        }

        public void MouseUp(Point position, Viewport viewport)
        {
            _isMouseDown = false;
        }
    }
}

using System.Windows;
using Entities;

namespace Viewing
{
    public class PanCameraInteractor : IMouseInteractor
    {
        private readonly Camera _camera;
        private bool _isMouseDown;
        private Matrix _initialTransform;
        private Point _initialPosition;

        public PanCameraInteractor(Camera camera)
        {
            _camera = camera;
        }

        public void MouseDown(Point position, Viewport viewport)
        {
            _isMouseDown = true;
            _initialTransform = _camera.ViewportPan;
            _initialPosition = position;
        }

        public bool MouseMove(Point position, Viewport viewport)
        {
            if (!_isMouseDown) return false;

            var delta = (position - _initialPosition) * 0.1;
            _camera.ViewportPan = Matrix.Translation(new Vector3(delta.X, -delta.Y, 0)) * _initialTransform;
            return true;
        }

        public void MouseUp(Point position, Viewport viewport)
        {
            _isMouseDown = false;
        }
    }
}

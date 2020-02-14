using Entities;

namespace Viewing
{
    public class Camera : Bindable
    {
        private Matrix _transformationToWorld = new Matrix();
        private Matrix _viewportPan = new Matrix();
        private double _zoom = 100;

        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        public Matrix ViewportPan
        {
            get => _viewportPan;
            set => SetProperty(ref _viewportPan, value);
        }

        public Matrix TransformationToWorld
        {
            get => _transformationToWorld;
            set => SetProperty(ref _transformationToWorld, value);
        }
    }
}

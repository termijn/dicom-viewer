using System;
using Entities;

namespace Viewing
{
    public class Camera : Bindable
    {
        private Matrix _transformationToWorld = new Matrix();
        private Matrix _viewportPan = new Matrix();
        private double _zoom = 100;

        public Camera(Space parentSpace)
        {
            Space = new Space(parentSpace);
        }

        public Space Space
        {
            get;
        }

        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        public Coordinate CenterPan
        {
            get;
            set;
        }

        public Matrix ViewportPan
        {
            get => _viewportPan;
            set => SetProperty(ref _viewportPan, value);
        }

        public Matrix GetTransformation()
        {
            var centering = new Matrix();
            if (CenterPan != null)
            {
                var offset = CenterPan.In(Space);
                offset.Z = 0;
                centering = Matrix.Translation(offset);
            }

            return Space.GetTransformationToRoot() * ViewportPan * centering;
        }
    }
}

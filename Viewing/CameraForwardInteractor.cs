using Entities;
using System.Windows;

namespace Viewing
{

    public class CameraForwardInteractor : DragMouseInteractor
    {
        private Matrix cameraTransformationAtBegin;

        protected override void OnMouseDown(Point position, Viewport viewport)
        {
            cameraTransformationAtBegin = viewport.Camera.TransformationToWorld;
        }

        protected override void OnMouseMove(Point position, Viewport viewport)
        {
            viewport.Camera.TransformationToWorld = cameraTransformationAtBegin * Matrix.Translation(0, 0, Delta.Y / 10);
        }
    }
}

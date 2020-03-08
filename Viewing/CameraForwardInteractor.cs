using Entities;
using System.Windows;

namespace Viewing
{

    public class ForwardInteractor : DragMouseInteractor
    {
        private Matrix transformationAtBegin;
        private Space _space;

        public ForwardInteractor(Space space)
        {
            _space = space;
        }

        protected override void OnMouseDown(Point position, Viewport viewport)
        {
            transformationAtBegin = _space.TransformationToParent;
        }

        protected override void OnMouseMove(Point position, Viewport viewport)
        {
            Coordinate c0 = viewport.Camera.Space.Origin;
            Coordinate c1 = new Coordinate(viewport.Camera.Space, new Vector3(0,0,-1));
            Vector3 v0 = c0.In(_space.Parent);
            Vector3 v1 = c1.In(_space.Parent);
            Vector3 direction = (v1 - v0).Normalized() * Delta.Y / 10.0;

            _space.TransformationToParent = transformationAtBegin * Matrix.Translation(direction);
        }
    }
}

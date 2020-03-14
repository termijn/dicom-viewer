using Entities;
using RenderEngine;
using System.Linq;
using System.Windows;

namespace Viewing
{
    public class AxesInteractor: IMouseInteractor 
    {
        private readonly Space _voiSpace;
        private bool _isDragging;
        private Vector3 _beginPosition;
        private Matrix _beginTransformation;

        public AxesInteractor(Space voiSpace)
        {
            _voiSpace = voiSpace;
        }

        public void MouseDown(Point position, Viewport viewport)
        {
            var visual = GetVisual(viewport);
            if (IsVoiCenterHit(position, viewport))
            {
                _isDragging = true;
                _beginPosition = viewport.ViewportRenderer.GetPositionInWorld(position.X, position.Y);
                _beginTransformation = _voiSpace.TransformationToParent;
                visual.Highlight();
            }
        }

        public bool MouseMove(Point position, Viewport viewport)
        {
            var visual = GetVisual(viewport);

            var newPosition = viewport.ViewportRenderer.GetPositionInWorld(position.X, position.Y);

            if (_isDragging)
            {
                var delta = newPosition - _beginPosition;

                var beginTranslation = _beginTransformation.Translation();
                var newTranslation = beginTranslation + delta;

                _voiSpace.TransformationToParent =
                    Matrix.Translation(newTranslation) *
                    Matrix.Translation(beginTranslation).Inverted() * 
                    _beginTransformation;

                return true;
            }
            else
            {
                if (IsVoiCenterHit(position, viewport))
                {
                    visual.Highlight();
                }
                else
                {
                    visual.DeHighlight();
                }
            }
            return false;
        }

        private bool IsVoiCenterHit(Point position, Viewport viewport)
        {
            var root = viewport.Camera.Space.GetRoot();
            var clickPositionInWorld = viewport.ViewportRenderer.GetPositionInWorld(position.X, position.Y);
            var viewingDirection = viewport.Camera.Space.AxisZ;
            var viewingDirectionInWorld = -viewingDirection.In(root);

            var ray = new Ray(clickPositionInWorld, viewingDirectionInWorld);
            var rayToOrigin = ray.PointToRay(_voiSpace.Origin.In(root));
            return rayToOrigin.Length() < 2;
        }

        public void MouseUp(Point position, Viewport viewport)
        {
            _isDragging = false;
            var visual = GetVisual(viewport);
            visual.DeHighlight();
        }

        private SlabAxesVisual GetVisual(Viewport viewport)
        {
            var visual = (SlabAxesVisual)viewport.Visuals.FirstOrDefault(v => v is SlabAxesVisual);
            return visual;
        }
    }
}

using System.Windows;

namespace Viewing
{
    public class MouseInteractorList: IMouseInteractor
    {
        private readonly IMouseInteractor[] _mouseInteractors;

        public MouseInteractorList(params IMouseInteractor[] mouseInteractors)
        {
            _mouseInteractors = mouseInteractors;
        }

        public void MouseDown(Point position, Viewport viewport)
        {
            foreach(var interactor in _mouseInteractors)
            {
                interactor.MouseDown(position, viewport);
            }
        }

        public bool MouseMove(Point position, Viewport viewport)
        {
            foreach (var interactor in _mouseInteractors)
            {
                if (interactor.MouseMove(position, viewport))
                {
                    break;
                }
            }
            return true;
        }

        public void MouseUp(Point position, Viewport viewport)
        {
            foreach (var interactor in _mouseInteractors)
            {
                interactor.MouseUp(position, viewport);
            }
        }
    }
}

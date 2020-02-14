using System.Windows;

namespace Viewing
{
    public interface IMouseInteractor
    {
        void MouseDown(Point position, Viewport viewport);
        bool MouseMove(Point position, Viewport viewport);
        void MouseUp(Point position, Viewport viewport);
    }
}

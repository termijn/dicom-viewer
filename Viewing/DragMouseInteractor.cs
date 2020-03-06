using System.Windows;

namespace Viewing
{
    public class DragMouseInteractor: IMouseInteractor
    {
        public bool IsDragging { get; private set; } = false;
        public Point StartPosition { get; private set; } = new Point();
        public Vector Delta { get; private set; } = new Vector();

        public void MouseDown(Point position, Viewport viewport)
        {
            StartPosition = position;
            IsDragging = true;
            OnMouseDown(position, viewport);
        }

        public bool MouseMove(Point position, Viewport viewport)
        {
            if (IsDragging)
            {
                Delta = StartPosition - position;
                OnMouseMove(position, viewport);
            }
            return IsDragging;
        }

        public void MouseUp(Point position, Viewport viewport)
        {
            OnMouseUp(position, viewport);
        }

        protected virtual void OnMouseDown(Point position, Viewport viewport)
        {
        }

        protected virtual void OnMouseMove(Point position, Viewport viewport)
        {
        }
        protected virtual void OnMouseUp(Point position, Viewport viewport)
        {
        }
    }
}

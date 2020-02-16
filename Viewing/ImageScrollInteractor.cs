using RenderEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Viewing
{
    public class ImageScrollInteractor : IMouseInteractor
    {
        private readonly ImageVisual imageVisual;
        private bool isMouseDown;
        private Point startPosition;
        private int startIndex;

        public ImageScrollInteractor(ImageVisual imageVisual)
        {
            this.imageVisual = imageVisual;
        }

        public void MouseDown(Point position, Viewport viewport)
        {
            isMouseDown = true;
            startPosition = position;
            startIndex = imageVisual.GetImageIndex();
        }

        public bool MouseMove(Point position, Viewport viewport)
        {
            if (isMouseDown)
            {
                var delta = position - startPosition;
                var newIndex = (int) (startIndex + delta.Y / 20);
                newIndex = Math.Min(Math.Max(newIndex, 0), imageVisual.GetNumberOfImages()-1);
                imageVisual.SetImageIndex(newIndex);

                return true;
            }
            return false;
        }

        public void MouseUp(Point position, Viewport viewport)
        {
            isMouseDown = false;
        }
    }
}

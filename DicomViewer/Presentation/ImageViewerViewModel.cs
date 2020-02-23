using Entities;
using RenderEngine;
using Viewing;

namespace DicomViewer.Presentation
{
    public class ImageViewerViewModel : Bindable, IInteractorActivator
    {
        private VisualsCollection _visuals = new VisualsCollection();
        private IMouseInteractor _interactorLeft;
        private IMouseInteractor _interactorRight;
        private Camera _camera;
        private double _windowLevel;
        private double _windowWidth;
        private double _min;
        private double _max;

        public ImageViewerViewModel()
        {
            Camera = new Camera();
            Tools = new ToolSelectorViewModel(this);
            InteractorRight = new PanCameraInteractor(Camera);
        }

        public double WindowLevel
        {
            get => _windowLevel;
            set
            {
                if (SetProperty(ref _windowLevel, value))
                {
                    ImageVisual?.SetWindowing(WindowLevel, WindowWidth);
                }
            }
        }
        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                if (SetProperty(ref _windowWidth, value))
                {
                    ImageVisual?.SetWindowing(WindowLevel, WindowWidth);
                }
            }
        }

        public double Min
        {
            get => _min; set => SetProperty(ref _min, value);
        }

        public double Max
        {
            get => _max; set => SetProperty(ref _max, value);
        }

        public VisualsCollection Visuals { get => _visuals; set => SetProperty(ref _visuals, value); }
        public ImageVisual ImageVisual { get; set; }
        public Camera Camera { get => _camera; set => SetProperty(ref _camera, value); }
        public IMouseInteractor InteractorLeft { get => _interactorLeft; set => SetProperty(ref _interactorLeft, value); }
        public IMouseInteractor InteractorRight { get => _interactorRight; set => SetProperty(ref _interactorRight, value); }
        public ToolSelectorViewModel Tools { get; }

        public void ActivatePan()
        {
            InteractorLeft = new PanCameraInteractor(Camera);
        }

        public void ActivateRotate()
        {
            InteractorLeft = new RotateCameraInteractor(Camera);
        }

        public void ActivateScroll()
        {
            InteractorLeft = new ImageScrollInteractor(ImageVisual);
        }

        public void ActivateWindowing()
        {
            InteractorLeft = new ImageWindowingInteractor { ImageVisual = ImageVisual };
        }

        public void ActivateZoom()
        {
            InteractorLeft = new ZoomInteractor(Camera);
        }
    }
}

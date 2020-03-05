using Entities;
using RenderEngine;
using Viewing;

namespace DicomViewer.Presentation
{
    public class VolumeViewerViewModel : Bindable, IInteractorActivator
    {
        private IMouseInteractor _interactorLeft;
        private IMouseInteractor _interactorRight;
        private double _windowLevel;
        private double _windowWidth;
        private int[] _histogram;

        public VolumeViewerViewModel()
        {
            Tools = new ToolSelectorViewModel(this);
            WindowLevel = 1000;
            WindowWidth = 500;
        }

        public double WindowLevel
        {
            get => _windowLevel;
            set
            {
                if (SetProperty(ref _windowLevel, value))
                {
                    VolumeVisual?.SetWindowing(WindowLevel, WindowWidth);
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
                    VolumeVisual?.SetWindowing(WindowLevel, WindowWidth);
                }
            }
        }

        public int[] Histogram
        {
            get => _histogram; set => SetProperty(ref _histogram, value);
        }

        public VolumeVisual VolumeVisual { get; set; }
        public Camera Camera { get; } = new Camera();
        public IMouseInteractor InteractorLeft { get => _interactorLeft; set => SetProperty(ref _interactorLeft, value); }
        public IMouseInteractor InteractorRight { get => _interactorRight; set => SetProperty(ref _interactorRight, value); }
        public VisualsCollection Visuals { get; } = new VisualsCollection();
        public ToolSelectorViewModel Tools { get; }

        public void ActivatePan()
        {
            InteractorLeft = new PanCameraInteractor(Camera);
        }

        public void ActivateRotate()
        {
            InteractorLeft = new RotateCameraInteractor();
        }

        public void ActivateScroll()
        {
            // Scroll is not available for volume viewing
        }

        public void ActivateWindowing()
        {
            InteractorLeft = new WindowingInteractor { VolumeVisual = VolumeVisual };
        }

        public void ActivateZoom()
        {
            InteractorLeft = new ZoomInteractor(Camera);
        }
    }
}

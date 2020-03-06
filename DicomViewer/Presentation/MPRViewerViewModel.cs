using Entities;
using RenderEngine;
using Viewing;

namespace DicomViewer.Presentation
{
    public class MPRViewerViewModel: Bindable, IInteractorActivator
    {
        private IMouseInteractor _interactorLeft;
        private IMouseInteractor _interactorRight;
        private double _windowLevel;
        private double _windowWidth;

        public MPRViewerViewModel()
        {
            Tools = new ToolSelectorViewModel(this);
        }

        public VisualsCollection VisualsAxial { get; } = new VisualsCollection();
        public VisualsCollection VisualsCoronal { get; } = new VisualsCollection();
        public VisualsCollection VisualsSagital { get; } = new VisualsCollection();

        public Camera CameraAxial { get; } = new Camera();
        public Camera CameraCoronal { get; } = new Camera();
        public Camera CameraSagital { get; } = new Camera();
        
        public IMouseInteractor InteractorLeft { get => _interactorLeft; set => SetProperty(ref _interactorLeft, value); }
        public IMouseInteractor InteractorRight { get => _interactorRight; set => SetProperty(ref _interactorRight, value); }

        public SlabVisual SlabAxial { get; set; }
        public SlabVisual SlabSagital { get; set; }
        public SlabVisual SlabCoronal { get; set; }

        public ToolSelectorViewModel Tools { get; }

        public double WindowLevel
        {
            get => _windowLevel;
            set
            {
                if (SetProperty(ref _windowLevel, value))
                {
                    UpdateWindowing();
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
                    UpdateWindowing();
                }
            }
        }

        private void UpdateWindowing()
        {
            SlabAxial?.SetWindowing(WindowLevel, WindowWidth);
            SlabCoronal?.SetWindowing(WindowLevel, WindowWidth);
            SlabSagital?.SetWindowing(WindowLevel, WindowWidth);
        }

        public void ActivatePan()
        {
            InteractorLeft = new PanCameraInteractor();
        }

        public void ActivateRotate()
        {
            InteractorLeft = new RotateCameraInteractor();
        }

        public void ActivateScroll()
        {
            InteractorLeft = new CameraForwardInteractor();
        }

        public void ActivateWindowing()
        {
        }

        public void ActivateZoom()
        {
            InteractorLeft = new ZoomInteractor();
        }
    }
}

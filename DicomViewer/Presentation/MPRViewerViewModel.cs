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
            var root = new Space();
            VoiSpace = new Space(root);
            CameraAxial = new Camera(VoiSpace);
            CameraCoronal = new Camera(VoiSpace);
            CameraSagital = new Camera(VoiSpace);

            Tools = new ToolSelectorViewModel(this);
        }

        public VisualsCollection VisualsAxial { get; } = new VisualsCollection();
        public VisualsCollection VisualsCoronal { get; } = new VisualsCollection();
        public VisualsCollection VisualsSagital { get; } = new VisualsCollection();

        public Space VoiSpace { get; }

        public Camera CameraAxial { get; }
        public Camera CameraCoronal { get; }
        public Camera CameraSagital { get; }
        
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

        private MouseInteractorList CreateInteractorList(IMouseInteractor fallbackInteractor)
        {
            return new MouseInteractorList(new AxesInteractor(VoiSpace), fallbackInteractor);
        }

        public void ActivatePan()
        {
            InteractorLeft = CreateInteractorList(new PanCameraInteractor());
        }

        public void ActivateRotate()
        {
            InteractorLeft = CreateInteractorList(new RotateInteractor(VoiSpace));
        }

        public void ActivateScroll()
        {
            InteractorLeft = CreateInteractorList(new ForwardInteractor(VoiSpace));
        }

        public void ActivateWindowing()
        {
        }

        public void ActivateZoom()
        {
            InteractorLeft = CreateInteractorList(new ZoomInteractor());
        }
    }
}

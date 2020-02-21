using Entities;
using RenderEngine;
using Viewing;

namespace DicomViewer.Presentation
{
    public class VolumeViewerViewModel: Bindable, IInteractorActivator
    {
        private IMouseInteractor _interactorLeft;
        private IMouseInteractor _interactorRight;

        public VolumeViewerViewModel()
        {
            Tools = new ToolSelectorViewModel(this);
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
            InteractorLeft = new RotateCameraInteractor(Camera);
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

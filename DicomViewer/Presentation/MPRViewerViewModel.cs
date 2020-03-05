using Entities;
using RenderEngine;
using Viewing;

namespace DicomViewer.Presentation
{
    public class MPRViewerViewModel: Bindable
    {
        private IMouseInteractor _interactorLeft;
        private IMouseInteractor _interactorRight;

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
    }
}

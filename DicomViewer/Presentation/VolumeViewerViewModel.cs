using Entities;
using RenderEngine;
using Viewing;

namespace DicomViewer.Presentation
{
    public class VolumeViewerViewModel: Bindable
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
    }
}

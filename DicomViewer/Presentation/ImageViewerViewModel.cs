using Entities;
using Viewing;

namespace DicomViewer.Presentation
{
    public class ImageViewerViewModel : Bindable
    {
        private VisualsCollection _visuals = new VisualsCollection();
        private IMouseInteractor _interactorLeft;
        private Camera _camera;

        public ImageViewerViewModel()
        {
            Camera = new Camera();
        }

        public VisualsCollection Visuals
        {
            get => _visuals; set => SetProperty(ref _visuals, value);
        }

        public Camera Camera { get => _camera; set => SetProperty(ref _camera, value); }

        public IMouseInteractor InteractorLeft { get => _interactorLeft; set => SetProperty(ref _interactorLeft, value); }
    }
}

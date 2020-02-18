using Entities;
using Viewing;

namespace DicomViewer.Presentation
{
    public class ImageViewerViewModel : Bindable
    {
        private VisualsCollection _visuals = new VisualsCollection();
        private IMouseInteractor _interactorLeft;

        public VisualsCollection Visuals
        {
            get => _visuals; set => SetProperty(ref _visuals, value);
        }

        public IMouseInteractor InteractorLeft { get => _interactorLeft; set => SetProperty(ref _interactorLeft, value); }
    }
}

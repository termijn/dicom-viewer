using Entities;
using Viewing;

namespace DicomViewer.Presentation
{
    public class ToolSelectorViewModel: Bindable
    {
        private readonly VolumeViewerViewModel volumeViewer;
        private bool _isZoomActive;
        private bool _isRotateActive = true;
        private bool _isPanActive;
        private bool _isWindowingActive;

        public ToolSelectorViewModel(VolumeViewerViewModel volumeViewer)
        {
            this.volumeViewer = volumeViewer;
        }

        public bool IsZoomActive
        {
            get => _isZoomActive;
            set
            {
                if (value)
                {
                    _isZoomActive = true;
                    _isRotateActive = false;
                    _isPanActive = false;
                    _isWindowingActive = false;
                    volumeViewer.InteractorLeft = new ZoomInteractor(volumeViewer.Camera);
                }
                RaiseAllPropertiesChangedEvent();
            }
        }
        public bool IsRotateActive
        {
            get => _isRotateActive;
            set
            {
                if (value)
                {
                    _isZoomActive = false;
                    _isRotateActive = true;
                    _isPanActive = false;
                    _isWindowingActive = false;
                    volumeViewer.InteractorLeft = new RotateCameraInteractor(volumeViewer.Camera);
                }
                RaiseAllPropertiesChangedEvent();
            }
        }

        public bool IsPanActive
        {
            get => _isPanActive;
            set
            {
                if (value)
                {
                    _isZoomActive = false;
                    _isRotateActive = false;
                    _isPanActive = true;
                    _isWindowingActive = false;
                    volumeViewer.InteractorLeft = new PanCameraInteractor(volumeViewer.Camera);
                }
                RaiseAllPropertiesChangedEvent();
            }
        }

        public bool IsWindowingActive
        {
            get => _isWindowingActive;
            set
            {
                if (value)
                {
                    _isZoomActive = false;
                    _isRotateActive = false;
                    _isPanActive = false;
                    _isWindowingActive = false;
                    volumeViewer.InteractorLeft = new WindowingInteractor { VolumeVisual = volumeViewer.VolumeVisual };
                }
                RaiseAllPropertiesChangedEvent();
            }
        }
    }
}

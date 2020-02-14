using Entities;
using Viewing;

namespace DicomViewer.Presentation
{
    public class ToolSelectorViewModel: Bindable
    {
        private readonly MainViewModel mainViewModel;
        private bool _isZoomActive;
        private bool _isRotateActive = true;
        private bool _isPanActive;
        private bool _isWindowingActive;

        public ToolSelectorViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
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
                    mainViewModel.InteractorLeft = new ZoomInteractor(mainViewModel.Camera);
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
                    mainViewModel.InteractorLeft = new RotateCameraInteractor(mainViewModel.Camera);
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
                    mainViewModel.InteractorLeft = new PanCameraInteractor(mainViewModel.Camera);
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
                    mainViewModel.InteractorLeft = new WindowingInteractor { VolumeVisual = mainViewModel.VolumeVisual };
                }
                RaiseAllPropertiesChangedEvent();
            }
        }
    }
}

using Entities;

namespace DicomViewer.Presentation
{

    public class ToolSelectorViewModel: Bindable
    {
        private readonly IInteractorActivator _activator;
        private bool _isZoomActive;
        private bool _isRotateActive = true;
        private bool _isPanActive;
        private bool _isWindowingActive;
        private bool _isScrollActive;

        public ToolSelectorViewModel(IInteractorActivator activator)
        {
            _activator = activator;
        }

        public bool IsScrollActive
        {
            get => _isScrollActive;
            set
            {
                if (value)
                {
                    DisableAll();
                    _isScrollActive = true;
                    _activator.ActivateScroll();
                }
                RaiseAllPropertiesChangedEvent();
            }
        }

        public bool IsZoomActive
        {
            get => _isZoomActive;
            set
            {
                if (value)
                {
                    DisableAll();
                    _isZoomActive = true;
                    _activator.ActivateZoom();
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
                    DisableAll();
                    _isRotateActive = true;
                    _activator.ActivateRotate();
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
                    DisableAll();
                    _isPanActive = true;
                    _activator.ActivatePan();
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
                    DisableAll();
                    _isWindowingActive = true;
                    _activator.ActivateWindowing();
                }
                RaiseAllPropertiesChangedEvent();
            }
        }

        private void DisableAll()
        {
            _isZoomActive = false;
            _isRotateActive = false;
            _isPanActive = false;
            _isWindowingActive = false;
            _isScrollActive = false;
        }
    }
}

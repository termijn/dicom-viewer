namespace DicomViewer.Presentation
{
    public interface IInteractorActivator
    {
        void ActivatePan();
        void ActivateRotate();
        void ActivateWindowing();
        void ActivateZoom();
        void ActivateScroll();
    }
}
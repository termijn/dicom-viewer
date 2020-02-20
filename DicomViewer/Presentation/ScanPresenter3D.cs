using DicomViewer.IO;
using Entities;
using RenderEngine;
using System;
using Viewing;

namespace DicomViewer.Presentation
{
    public class ScanPresenter3D: Disposable
    {
        private readonly MainViewModel _mainViewModel;
        private readonly VolumeViewerViewModel _volumeViewerViewModel;
        private VolumeVisual _visual;

        public ScanPresenter3D(MainViewModel mainViewModel, VolumeViewerViewModel volumeViewerViewModel)
        {
            _mainViewModel = mainViewModel;
            _volumeViewerViewModel = volumeViewerViewModel;
        }

        public void Present(Scan scan)
        {
            _mainViewModel.Patient = scan.Patient;

            _visual = new VolumeVisual(scan.Volume);
            _volumeViewerViewModel.Visuals.Add(_visual);

            _volumeViewerViewModel.Camera.TransformationToWorld = Matrix.Translation(scan.Volume.CenterInPatientSpace) * Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(1, 0, 0));
            _volumeViewerViewModel.InteractorLeft = new RotateCameraInteractor(_volumeViewerViewModel.Camera);
            _volumeViewerViewModel.InteractorRight = new PanCameraInteractor(_volumeViewerViewModel.Camera);
            _volumeViewerViewModel.VolumeVisual = _visual;
            _volumeViewerViewModel.Tools.IsRotateActive = true;
        }

        protected override void OnDispose()
        {
            _volumeViewerViewModel.Visuals.Clear();
            _visual.Dispose();
        }
    }
}

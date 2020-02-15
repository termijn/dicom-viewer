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
        private VolumeVisual _visual;

        public ScanPresenter3D(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public void Present(Scan3D scan)
        {
            _mainViewModel.Patient = scan.Patient;

            _visual = new VolumeVisual(scan.Volume);
            _mainViewModel.Visuals.Add(_visual);

            _mainViewModel.Camera.TransformationToWorld = Matrix.Translation(scan.Volume.CenterInPatientSpace) * Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(1, 0, 0));
            _mainViewModel.InteractorLeft = new RotateCameraInteractor(_mainViewModel.Camera);
            _mainViewModel.InteractorRight = new PanCameraInteractor(_mainViewModel.Camera);
            _mainViewModel.VolumeVisual = _visual;
            _mainViewModel.Tools.IsRotateActive = true;
        }

        protected override void OnDispose()
        {
            _mainViewModel.Visuals.Clear();
            _visual.Dispose();
        }
    }
}

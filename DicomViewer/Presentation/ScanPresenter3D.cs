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

            double marginInPixels = 48;
            double margin = marginInPixels * scan.Volume.VoxelSpacing.Y;
            _volumeViewerViewModel.Camera.Zoom = scan.Volume.VoxelSpacing.Y * scan.Volume.Dimensions.Y * 0.5 + margin;
            _volumeViewerViewModel.Camera.TransformationToWorld = Matrix.Translation(scan.Volume.CenterInPatientSpace) * Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(1, 0, 0));
            _volumeViewerViewModel.Tools.IsRotateActive = true;
            _volumeViewerViewModel.InteractorRight = new PanCameraInteractor(_volumeViewerViewModel.Camera);
            _volumeViewerViewModel.VolumeVisual = _visual;
            _volumeViewerViewModel.Tools.IsRotateActive = true;

            var centerSlice = scan.Volume.Slices[scan.Volume.Slices.Count / 2];
            _volumeViewerViewModel.WindowLevel = centerSlice.WindowLevel;
            _volumeViewerViewModel.WindowWidth = centerSlice.WindowWidth;
            var levelMin = centerSlice.WindowLevel - centerSlice.WindowWidth / 2;
            var levelMax = centerSlice.WindowLevel + centerSlice.WindowWidth / 2;

             _volumeViewerViewModel.WindowLevel = centerSlice.WindowLevel;
            _volumeViewerViewModel.WindowWidth = centerSlice.WindowWidth;
        }

        protected override void OnDispose()
        {
            _volumeViewerViewModel.Visuals.Remove(_visual);
            _visual.Dispose();
        }
    }
}

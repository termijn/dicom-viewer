using DicomViewer.IO;
using Entities;
using RenderEngine;
using System;
using Viewing;

namespace DicomViewer.Presentation
{
    public class ScanPresenterMPR: Disposable
    {
        private readonly MainViewModel viewModel;

        public ScanPresenterMPR(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public void Present(Scan scan)
        {
            double mmPerVoxel = scan.Volume.DimensionsInPatientSpace.Z / scan.Volume.NumberOfVoxelsInPatientSpace.Z;
            double marginInPixels = 12;
            double marginInmm = marginInPixels * mmPerVoxel;

            var viewer = viewModel.MPRViewer;

            viewer.VisualsAxial.Add(new SlabAxesVisual(viewer.VoiSpace, Plane.XY));
            viewer.VisualsCoronal.Add(new SlabAxesVisual(viewer.VoiSpace, Plane.XZ));
            viewer.VisualsSagital.Add(new SlabAxesVisual(viewer.VoiSpace, Plane.YZ));

            viewer.SlabAxial = new SlabVisual(scan.Volume);
            viewer.SlabSagital = new SlabVisual(scan.Volume);
            viewer.SlabCoronal = new SlabVisual(scan.Volume);

            viewer.VisualsAxial.Add(viewer.SlabAxial);
            viewer.VisualsSagital.Add(viewer.SlabSagital);
            viewer.VisualsCoronal.Add(viewer.SlabCoronal);

            viewer.VoiSpace.TransformationToParent = Matrix.Translation(scan.Volume.CenterInPatientSpace);

            viewer.CameraAxial.Zoom = scan.Volume.DimensionsInPatientSpace.Y * 0.5 + marginInmm;
            viewer.CameraAxial.ViewportPan = new Matrix();

            viewer.CameraSagital.Space.TransformationToParent = Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(0, 0, 1))  * Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(1, 0, 0));
            viewer.CameraSagital.Zoom = scan.Volume.DimensionsInPatientSpace.Z * 0.5 + marginInmm;
            viewer.CameraSagital.ViewportPan = new Matrix();

            viewer.CameraCoronal.Space.TransformationToParent = Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(1, 0, 0));
            viewer.CameraCoronal.Zoom = scan.Volume.DimensionsInPatientSpace.Z * 0.5 + marginInmm;
            viewer.CameraCoronal.ViewportPan = new Matrix();

            var root = viewer.CameraAxial.Space.GetRoot();
            viewer.CameraAxial.CenterPan = new Coordinate(root, scan.Volume.CenterInPatientSpace);
            viewer.CameraSagital.CenterPan = new Coordinate(root, scan.Volume.CenterInPatientSpace);
            viewer.CameraCoronal.CenterPan = new Coordinate(root, scan.Volume.CenterInPatientSpace);

            viewer.ActivateScroll();

            var centerSlice = scan.Volume.Slices[scan.Volume.Slices.Count / 2];
            viewer.WindowLevel = centerSlice.WindowLevel;
            viewer.WindowWidth = centerSlice.WindowWidth;
            var levelMin = centerSlice.WindowLevel - centerSlice.WindowWidth / 2;
            var levelMax = centerSlice.WindowLevel + centerSlice.WindowWidth / 2;

            viewer.WindowLevel = centerSlice.WindowLevel;
            viewer.WindowWidth = centerSlice.WindowWidth;
        }

        protected override void OnDispose()
        {
            var viewer = viewModel.MPRViewer;
            viewer.VisualsAxial.Clear();
            viewer.VisualsCoronal.Clear();
            viewer.VisualsSagital.Clear();

            viewer.SlabAxial.Dispose();
            viewer.SlabCoronal.Dispose();
            viewer.SlabSagital.Dispose();
        }
    }
}

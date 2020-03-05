using DicomViewer.IO;
using Entities;
using RenderEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            double marginInPixels = 14;
            double margin = marginInPixels * scan.Volume.VoxelSpacing.Y;

            var viewer = viewModel.MPRViewer;

            viewer.SlabAxial = new SlabVisual(scan.Volume);
            viewer.SlabSagital = new SlabVisual(scan.Volume);
            viewer.SlabCoronal = new SlabVisual(scan.Volume);

            viewer.VisualsAxial.Add(viewer.SlabAxial);
            viewer.VisualsSagital.Add(viewer.SlabSagital);
            viewer.VisualsCoronal.Add(viewer.SlabCoronal);

            viewer.CameraAxial.TransformationToWorld = Matrix.Translation(scan.Volume.CenterInPatientSpace);
            viewer.CameraAxial.Zoom = scan.Volume.VoxelSpacing.Z * scan.Volume.Dimensions.Z * 0.5 + margin;
            viewer.CameraAxial.ViewportPan = new Matrix();

            viewer.CameraSagital.TransformationToWorld = Matrix.Translation(scan.Volume.CenterInPatientSpace) * Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(0, 0, 1))  * Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(1, 0, 0));
            viewer.CameraSagital.Zoom = scan.Volume.VoxelSpacing.Y * scan.Volume.Dimensions.Y * 0.5 + margin;
            viewer.CameraSagital.ViewportPan = new Matrix();

            viewer.CameraCoronal.TransformationToWorld = Matrix.Translation(scan.Volume.CenterInPatientSpace) * Matrix.RotationAngleAxis(-Math.PI / 2, new Vector3(1, 0, 0));
            viewer.CameraCoronal.Zoom = scan.Volume.VoxelSpacing.Y * scan.Volume.Dimensions.Y * 0.5 + margin;
            viewer.CameraCoronal.ViewportPan = new Matrix();

            viewer.InteractorLeft = new RotateCameraInteractor();
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

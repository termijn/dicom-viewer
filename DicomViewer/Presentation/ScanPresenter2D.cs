using DicomViewer.IO;
using Entities;
using RenderEngine;
using System;

namespace DicomViewer.Presentation
{
    public class ScanPresenter2D: Disposable
    {
        private readonly MainViewModel viewModel;
        private ImageVisual _imageVisual;

        public ScanPresenter2D(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public void Present(Scan scan)
        {
            var imageViewer = viewModel.ImageViewer;
            var firstImage = scan.Volume.Slices[scan.Volume.Slices.Count / 2];
            _imageVisual = new ImageVisual(scan.Volume);
            imageViewer.ImageVisual = _imageVisual;
            imageViewer.Visuals.Add(_imageVisual);

            imageViewer.Tools.IsScrollActive = true;
            imageViewer.Camera.Zoom = firstImage.Height * firstImage.PixelSpacing.Y * 0.5;
            imageViewer.Camera.ViewportPan = Matrix.Translation(new Vector3(firstImage.Width * firstImage.PixelSpacing.X / 2, firstImage.Height * firstImage.PixelSpacing.Y / 2, 0));

            imageViewer.WindowLevel = firstImage.WindowLevel;
            imageViewer.WindowWidth = firstImage.WindowWidth;
            var levelMin = firstImage.WindowLevel - firstImage.WindowWidth / 2;
            var levelMax = firstImage.WindowLevel + firstImage.WindowWidth / 2;
            imageViewer.Min = Math.Min(levelMin, firstImage.MinRescaledValue);
            imageViewer.Max = Math.Max(levelMax, firstImage.MaxRescaledValue);
            var range = imageViewer.Max - imageViewer.Min;
            imageViewer.Min -= range / 2;
            imageViewer.Max += range / 2;
            viewModel.SwitchTo2DCommand.Execute(null);
        }

        protected override void OnDispose()
        {
            viewModel.ImageViewer.Visuals.Remove(_imageVisual);
            _imageVisual.Dispose();
        }
    }
}

using Dicom.Imaging;
using Dicom.Log;
using DicomViewer.DotNetExtensions;
using DicomViewer.IO;
using DicomViewer.Presentation;
using DicomViewer.Properties;
using Entities;
using Microsoft.WindowsAPICodePack.Dialogs;
using RenderEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Viewing;

namespace DicomViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainViewModel _viewModel;
        private Scan _scan;
        private ScanPresenter3D _presenter;
        private ImageVisual _imageVisual;

        protected override void OnStartup(StartupEventArgs e)
        {
            LogManager.SetImplementation(ConsoleLogManager.Instance);
            ImageManager.SetImplementation(WPFImageManager.Instance);
        }
        protected override void OnActivated(EventArgs e)
        {
            if (_viewModel != null) { return; }
            _viewModel = (MainViewModel)MainWindow.DataContext;

            _viewModel.LoadDatasetCommand = new BindableCommand(LoadFolder);
            _viewModel.LoadFileCommand = new BindableCommand(LoadFile);

            new PropertySubscription(() => _viewModel.SelectedSeries, () =>
            {
                LoadSeries();
            });
        }

        private void LoadSeries()
        {
            _viewModel.ImageViewer.Visuals.Clear();
            DisposeScan();
            if (_viewModel.SelectedSeries == null) { return; }

            _scan = DicomVolumeLoader.Load(_viewModel.SelectedSeries as DicomSeries);
            _presenter = new ScanPresenter3D(_viewModel, _viewModel.VolumeViewer);
            _presenter.Present(_scan);

            _imageVisual = new ImageVisual(_scan.Volume.Slices);
            _viewModel.ImageViewer.ImageVisual = _imageVisual;
            _viewModel.ImageViewer.Visuals.Add(_imageVisual);
            _viewModel.ImageViewer.Tools.IsScrollActive = true;
            _viewModel.ImageViewer.Camera.Zoom = _scan.Volume.Slices.First().Height * _scan.Volume.Slices.First().PixelSpacing.Y * 0.5;
            _viewModel.ImageViewer.Camera.ViewportPan = new Matrix();
            _viewModel.SwitchTo2DCommand.Execute(null);
        }

        private void LoadFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                InitialDirectory = Settings.Default.LastUsedFolder
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Open(dialog.FileName);
            }
        }

        private void LoadFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = Settings.Default.LastUsedFolder
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Open(dialog.FileName);
            }
        }

        private void Open(string path)
        {
            _viewModel.ImageViewer.Visuals.Clear();

            DisposeScan();

            IEnumerable<Series> series = null;
            if (Directory.Exists(path))
            {
                series = DicomSeriesExtractor.ExtractSeriesFromDirectory(path);
            }
            else
            {
                if (string.Compare(Path.GetFileName(path), "DICOMDIR", true, CultureInfo.InvariantCulture) == 0)
                {
                    series = DicomSeriesExtractor.ExtractSeriesFromDicomDir(path);
                }
                else
                {
                    series = DicomSeriesExtractor.ExtractSeriesFromSingleFile(path);
                }
            }
            _viewModel.Series = series;
            _viewModel.SelectedSeries = series.First();

            Settings.Default.LastUsedFolder = path;
            Settings.Default.Save();
        }

        private void DisposeScan()
        {
            if (_imageVisual != null)
            {
                _imageVisual.Dispose();
                _imageVisual = null;
            }

            if (_presenter != null)
            {
                _presenter.Dispose();
                _presenter = null;
            }
            if (_scan != null)
            {
                _scan.Volume.Slices.ForEach(slice => slice.Dispose());
                _scan.Volume.Slices.Clear();
                _scan = null;
            }
        }
    }
}

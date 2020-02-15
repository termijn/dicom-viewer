using Dicom.Log;
using DicomViewer.DotNetExtensions;
using DicomViewer.IO;
using DicomViewer.Presentation;
using DicomViewer.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;

namespace DicomViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainViewModel _viewModel;
        private Scan3D _scan;
        private ScanPresenter3D _presenter;

        protected override void OnStartup(StartupEventArgs e)
        {
            LogManager.SetImplementation(ConsoleLogManager.Instance);
        }
        protected override void OnActivated(EventArgs e)
        {
            if (_viewModel != null) return;
            _viewModel = (MainViewModel)MainWindow.DataContext;
            _viewModel.LoadDatasetCommand = new BindableCommand(LoadFolder);
            _viewModel.LoadFileCommand = new BindableCommand(LoadFile);
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
                var loader = new DicomVolumeLoader();

                DisposeScan();

                _scan = loader.Load(dialog.FileName);

                if (_scan == null) return;

                _presenter = new ScanPresenter3D(_viewModel);
                _presenter.Present(_scan);

                Settings.Default.LastUsedFolder = dialog.FileName;
                Settings.Default.Save();
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
                var loader = new DicomVolumeLoader();

                DisposeScan();

                _scan = loader.Load(dialog.FileName);

                _presenter = new ScanPresenter3D(_viewModel);
                _presenter.Present(_scan);

                Settings.Default.LastUsedFolder = dialog.FileName;
                Settings.Default.Save();
            }
        }

        private void DisposeScan()
        {
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

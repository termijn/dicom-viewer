using Dicom.Imaging;
using Dicom.Log;
using DicomViewer.DotNetExtensions;
using DicomViewer.IO;
using DicomViewer.Presentation;
using DicomViewer.Properties;
using Entities;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;
using System.Windows.Threading;

namespace DicomViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainViewModel _viewModel;
        private ILogger _logger;
        private DataSetSelector _dataSetSelector;

        protected override void OnStartup(StartupEventArgs e)
        {
            LogManager.SetImplementation(ConsoleLogManager.Instance);
            ImageManager.SetImplementation(WPFImageManager.Instance);
            _logger = Logging.GetLogger<App>();
            _logger.LogInformation("Dicom Viewer started");

            Current.DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, $"'{e.Exception.Message}' was unhandled \n {e.Exception.StackTrace}");
        }

        protected override void OnActivated(EventArgs e)
        {
            if (_viewModel != null) { return; }
            _viewModel = (MainViewModel)MainWindow.DataContext;

            _viewModel.LoadDatasetCommand = new BindableCommand(LoadFolder);
            _viewModel.LoadFileCommand = new BindableCommand(LoadFile);

            new PropertySubscription(() => _viewModel.SelectedSeries, () =>
            {
                _dataSetSelector?.LoadSeries();
            });

            MainWindow.Drop += OnDrop;
            MainWindow.AllowDrop = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _dataSetSelector?.Dispose();
                _dataSetSelector = new DataSetSelector(_viewModel);
                _dataSetSelector.Open(files[0]);
            }
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
                _dataSetSelector?.Dispose();
                _dataSetSelector = new DataSetSelector(_viewModel);
                _dataSetSelector.Open(dialog.FileName);
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
                _dataSetSelector?.Dispose();
                _dataSetSelector = new DataSetSelector(_viewModel);
                _dataSetSelector.Open(dialog.FileName);
            }
        }
    }
}

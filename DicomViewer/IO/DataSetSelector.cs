using DicomViewer.Presentation;
using DicomViewer.Properties;
using Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DicomViewer.IO
{

    public class DataSetSelector: Disposable
    {
        private MainViewModel _viewModel;
        private Scan _scan;
        private ILogger _logger;
        private ScanPresenter2D _presenter2d;
        private ScanPresenter3D _presenter;

        public DataSetSelector(MainViewModel viewModel)
        {
            _logger = Logging.GetLogger<App>();
            _viewModel = viewModel;
        }

        public void LoadSeries()
        {
            if (_viewModel.SelectedSeries == null) { return; }
            RemovePresentedElements();

            _scan = DicomVolumeLoader.Load(_viewModel.SelectedSeries as DicomSeries);
            if (_scan == null) { return; }
            _presenter = new ScanPresenter3D(_viewModel, _viewModel.VolumeViewer);
            _presenter.Present(_scan);

            _presenter2d = new ScanPresenter2D(_viewModel);
            _presenter2d.Present(_scan);
        }

        public void Open(string path)
        {
            IEnumerable<Series> series = null;
            if (Directory.Exists(path))
            {
                _logger.LogInformation("User opened directory");
                series = DicomSeriesExtractor.ExtractSeriesFromDirectory(path);
            }
            else
            {
                if (string.Compare(Path.GetFileName(path), "DICOMDIR", true, CultureInfo.InvariantCulture) == 0)
                {
                    _logger.LogInformation("Opened dicomdir");
                    series = DicomSeriesExtractor.ExtractSeriesFromDicomDir(path);
                }
                else
                {
                    _logger.LogInformation("Opened single dicom file");
                    series = DicomSeriesExtractor.ExtractSeriesFromSingleFile(path);
                }
            }

            if (series == null || series.Count() == 0)
            {
                _logger.LogWarning("Series null or empty");
                return;
            }

            _viewModel.Series = series;
            _viewModel.SelectedSeries = series.First();

            Settings.Default.LastUsedFolder = path;
            Settings.Default.Save();
        }

        private void RemovePresentedElements()
        {
            if (_presenter2d != null)
            {
                _presenter2d.Dispose();
                _presenter2d = null;
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

        protected override void OnDispose()
        {
            RemovePresentedElements();
        }
    }
}

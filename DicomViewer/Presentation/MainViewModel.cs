using DicomViewer.DotNetExtensions;
using Entities;
using RenderEngine;
using System.Collections.Generic;

namespace DicomViewer.Presentation
{
    public class MainViewModel : Bindable
    {
        private Patient _patient;
        private BindableCommand _loadDatasetCommand;
        private BindableCommand _loadFileCommand;
        private IEnumerable<Series> _series;
        private Series _selectedSeries;
        private object _currentViewer;
        private BindableCommand _switchTo2DCommand;
        private BindableCommand _switchTo3DCommand;

        public MainViewModel()
        {
            SwitchTo2DCommand = new BindableCommand(SwitchTo2D);
            SwitchTo3DCommand = new BindableCommand(SwitchTo3D);

            VolumeViewer = new VolumeViewerViewModel();
            ImageViewer = new ImageViewerViewModel();
            CurrentViewer = VolumeViewer;
        }

        public VolumeViewerViewModel VolumeViewer { get; }
        public ImageViewerViewModel ImageViewer { get; }
        
        public IEnumerable<Series> Series { get => _series; set => SetProperty(ref _series, value); }

        public Patient Patient
        {
            get => _patient;
            set => SetProperty(ref _patient, value);
        }

        public object CurrentViewer
        {
            get => _currentViewer; set => SetProperty(ref _currentViewer, value);
        }

        public Series SelectedSeries { get => _selectedSeries; set => SetProperty(ref _selectedSeries, value); }

        public BindableCommand LoadDatasetCommand { get => _loadDatasetCommand; set => SetProperty(ref _loadDatasetCommand, value); }
        public BindableCommand LoadFileCommand { get => _loadFileCommand; set => SetProperty(ref _loadFileCommand, value); }

        public BindableCommand SwitchTo2DCommand { get => _switchTo2DCommand; set => SetProperty(ref _switchTo2DCommand, value); }
        public BindableCommand SwitchTo3DCommand { get => _switchTo3DCommand; set => SetProperty(ref _switchTo3DCommand, value); }
        
        private void SwitchTo3D()
        {
            CurrentViewer = VolumeViewer;
        }

        private void SwitchTo2D()
        {
            CurrentViewer = ImageViewer;
        }
    }
}

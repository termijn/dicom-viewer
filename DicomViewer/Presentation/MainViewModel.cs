using DicomViewer.DotNetExtensions;
using Entities;
using RenderEngine;
using System.Collections.Generic;
using Viewing;

namespace DicomViewer.Presentation
{
    public class MainViewModel : Bindable
    {
        private Patient _patient;
        private BindableCommand _loadDatasetCommand;
        private IMouseInteractor _interactorLeft;
        private IMouseInteractor _interactorRight;
        private BindableCommand _loadFileCommand;
        private IEnumerable<Series> _series;
        private Series _selectedSeries;

        public MainViewModel()
        {
            Tools = new ToolSelectorViewModel(this);
        }

        public IEnumerable<Series> Series { get => _series; set => SetProperty(ref _series, value); }

        public Patient Patient
        {
            get => _patient;
            set => SetProperty(ref _patient, value);
        }

        public VolumeVisual VolumeVisual { get; set; }

        public Series SelectedSeries { get => _selectedSeries; set => SetProperty(ref _selectedSeries, value); }

        public Camera Camera { get; } = new Camera();

        public IMouseInteractor InteractorLeft { get => _interactorLeft; set => SetProperty(ref _interactorLeft, value); }
        public IMouseInteractor InteractorRight { get => _interactorRight; set => SetProperty(ref _interactorRight, value); }

        public VisualsCollection Visuals { get; } = new VisualsCollection();

        public BindableCommand LoadDatasetCommand { get => _loadDatasetCommand; set => SetProperty(ref _loadDatasetCommand, value); }
        public BindableCommand LoadFileCommand { get => _loadFileCommand; set => SetProperty(ref _loadFileCommand, value); }

        public ToolSelectorViewModel Tools { get; }
    }
}

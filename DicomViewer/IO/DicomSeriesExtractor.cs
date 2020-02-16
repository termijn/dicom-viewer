using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dicom;
using Dicom.Imaging;
using Entities;

namespace DicomViewer.IO
{
    public class DicomSeriesExtractor
    {
        private readonly List<DicomSeries> _series = new List<DicomSeries>();

        public DicomSeriesExtractor()
        {

        }

        public IEnumerable<Series> ExtractSeries(string path)
        {
            var files = Directory.EnumerateFiles(path);
            var dicomFiles = files.Where(file => DicomFile.HasValidHeader(file));
            foreach(var file in dicomFiles)
            {
                var dicomFile = DicomFile.Open(file, FileReadOption.ReadLargeOnDemand);
                ProcessFile(dicomFile);
            }

            foreach(var series in _series)
            {
                var firstFile = series.Files.First();

                var dicomPixelData = DicomPixelData.Create(firstFile.Dataset);
                var nrFrames = dicomPixelData.NumberOfFrames;
                if (nrFrames > 1)
                {
                    series.NumberOfImages = nrFrames;
                }
                else
                {
                    series.NumberOfImages = series.Files.Count;
                }                
                series.Is3D = Is3DCapableSopClass(firstFile) && (series.FileNames.Count > 1 || nrFrames > 1);
                
                for (int i = series.Files.Count / 2; i >= 0; i--)
                {
                    var middleFile = series.Files[i];
                    if (middleFile.Dataset.Contains(DicomTag.PixelData))
                    {
                        DicomImage image = new DicomImage(middleFile.Dataset);
                        var renderedImage = image.RenderImage();
                        series.Thumbnail = renderedImage.AsWriteableBitmap();
                        break;
                    }
                }                
            }
            return _series;
        }

        public void ProcessFile(DicomFile file)
        {
            string seriesInstanceUid = file.Dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
            if (string.IsNullOrEmpty(seriesInstanceUid)) return;

            CreateSeries(file, seriesInstanceUid);
        }

        private void CreateSeries(DicomFile file, string seriesInstanceUid) 
        {            
            if (!IsSupportedSopClass(file))
            {
                return;
            }

            var series = _series.FirstOrDefault(s => s.SeriesInstanceUid == seriesInstanceUid);
            if (series == null)
            {
                var sopclass = file.Dataset.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);
                var seriesNumber = file.Dataset.GetValueOrDefault(DicomTag.SeriesNumber, 0, string.Empty);
                
                series = new DicomSeries
                {
                    SeriesInstanceUid = seriesInstanceUid,
                    SopClassUid = sopclass,
                    Number = seriesNumber   
                };
                _series.Add(series);
            }
            series.FileNames.Add(file.File.Name);
            series.Files.Add(file);
        }

        public bool IsSupportedSopClass(DicomFile file)
        {
            var sopclass = file.Dataset.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);
            return
                sopclass == DicomSopClasses.CTImageStorageSopClass ||
                sopclass == DicomSopClasses.EnhancedMRImageStorageSopClass ||
                sopclass == DicomSopClasses.MRImageStorageSopClass ||
                sopclass == DicomSopClasses.XA3DImageStorageSopClass;
        }

        public bool Is3DCapableSopClass(DicomFile file)
        {
            var sopclass = file.Dataset.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);

            bool mr3d = 
                sopclass == DicomSopClasses.MRImageStorageSopClass || 
                sopclass == DicomSopClasses.EnhancedMRImageStorageSopClass;
            //var isVolumetric = file.Dataset.GetValueOrDefault(DicomTag.VolumetricProperties, 0, string.Empty) == "VOLUME";

            return
                sopclass == DicomSopClasses.XA3DImageStorageSopClass ||
                sopclass == DicomSopClasses.CTImageStorageSopClass ||
                (mr3d);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dicom;
using Dicom.Imaging;
using Dicom.Media;
using Entities;

namespace DicomViewer.IO
{
    public class DicomSeriesExtractor
    {
        private readonly List<DicomSeries> _series = new List<DicomSeries>();

        public DicomSeriesExtractor()
        {

        }

        public IEnumerable<Series> ExtractSeriesFromDicomDir(string path)
        {
            if (!DicomDirectory.HasValidHeader(path))
            {
                return new List<Series>();
            }
            var dicomDirectory = DicomDirectory.Open(path);

            foreach (var patientRecord in dicomDirectory.RootDirectoryRecordCollection)
            {
                foreach (var studyRecord in patientRecord.LowerLevelDirectoryRecordCollection)
                {
                    foreach (var seriesRecord in studyRecord.LowerLevelDirectoryRecordCollection)
                    {
                        var seriesRecordType = seriesRecord.GetSingleValueOrDefault(DicomTag.DirectoryRecordType, string.Empty);
                        if (seriesRecordType != "SERIES") continue;
                        var series = new DicomSeries
                        {
                            SeriesInstanceUid = seriesRecord.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, ""),
                            Number = seriesRecord.GetSingleValueOrDefault(DicomTag.SeriesNumber, ""),
                        };

                        DicomSequence iconImageSequence;

                        if (seriesRecord.TryGetSequence(DicomTag.IconImageSequence, out iconImageSequence))
                        {
                            var iconImage = iconImageSequence.FirstOrDefault();
                            if (iconImage != null)
                            {
                                DicomImage image = new DicomImage(iconImage);
                                series.Thumbnail = image.RenderImage().AsWriteableBitmap();
                            }
                        }
                        foreach (var imageRecord in seriesRecord.LowerLevelDirectoryRecordCollection)
                        {
                            var imageRecordType = imageRecord.GetSingleValueOrDefault(DicomTag.DirectoryRecordType, string.Empty);
                            if (imageRecordType != "IMAGE") continue;

                            var values = imageRecord.GetValues<string>(DicomTag.ReferencedFileID);
                            var relativePath = string.Join(@"\", values);

                            series.SopClassUid = imageRecord.GetSingleValueOrDefault(DicomTag.ReferencedSOPClassUIDInFile, "");
                            series.NumberOfImages += imageRecord.GetSingleValueOrDefault(DicomTag.NumberOfFrames, 0);
                            var absolutePath = Path.GetDirectoryName(path) + @"\\" + relativePath;
                            series.FileNames.Add(absolutePath);
                        }
                        if (series.NumberOfImages > 0)
                        {
                            _series.Add(series);
                        }
                    }
                }
            }
            return _series;
        }


        private void ShowImageLevelInfo(DicomDataset dataset)
        {
            var values = dataset.GetValues<string>(DicomTag.ReferencedFileID);
            var referencedFileId = string.Join(@"\", values);
            var sopClassUidInFile = dataset.GetValue<string>(DicomTag.ReferencedSOPClassUIDInFile, 0);
            var sopInstanceUidInFile = dataset.GetValue<string>(DicomTag.ReferencedSOPInstanceUIDInFile, 0);
            var transferSyntaxUidInFile = dataset.GetValue<string>(DicomTag.ReferencedTransferSyntaxUIDInFile, 0);
        }

        public IEnumerable<Series> ExtractSeriesFromSingleFile(string path)
        {
            var dicomFile = DicomFile.Open(path, FileReadOption.ReadLargeOnDemand);
            ProcessFile(dicomFile);
            PostProcessSeries();
            return _series;
        }

        public IEnumerable<Series> ExtractSeriesFromDirectory(string path)
        {
            var files = Directory.EnumerateFiles(path);
            var dicomFiles = files.Where(file => DicomFile.HasValidHeader(file));
            foreach(var file in dicomFiles)
            {
                var dicomFile = DicomFile.Open(file, FileReadOption.ReadLargeOnDemand);
                ProcessFile(dicomFile);
            }
            PostProcessSeries();
            
            return _series;
        }

        private void PostProcessSeries()
        {
            foreach (var series in _series)
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

            return
                sopclass == DicomSopClasses.XA3DImageStorageSopClass ||
                sopclass == DicomSopClasses.CTImageStorageSopClass ||
                (mr3d);
        }
    }
}

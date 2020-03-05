using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dicom;
using Dicom.Imaging;
using Dicom.Media;
using Entities;
using Microsoft.Extensions.Logging;

namespace DicomViewer.IO
{
    public static class DicomSeriesExtractor
    {
        private static ILogger logger;
        private static readonly string LogCategory = "DicomSeriesExtractor";

        static DicomSeriesExtractor()
        {
            logger = Logging.GetLogger(LogCategory);
        }

        public static IEnumerable<Series> ExtractSeriesFromDicomDir(string path)
        {
            return ExtractSeriesFromDicomDir(path, false);
        }

        public static IEnumerable<Series> ExtractSeriesFromDicomDir(string path, bool generateMissingThumbnails)
        {
            var result = new List<DicomSeries>();
            if (!DicomFile.HasValidHeader(path))
            {
                return result;
            }

            try
            {
                var dicomDirectory = DicomDirectory.Open(path);

                foreach (var patientRecord in dicomDirectory.RootDirectoryRecordCollection)
                {
                    foreach (var studyRecord in patientRecord.LowerLevelDirectoryRecordCollection)
                    {
                        foreach (var seriesRecord in studyRecord.LowerLevelDirectoryRecordCollection)
                        {
                            var seriesRecordType = seriesRecord.GetSingleValueOrDefault(DicomTag.DirectoryRecordType, string.Empty);
                            if (seriesRecordType != "SERIES") { continue; }
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
                                if (imageRecordType != "IMAGE") { continue; }

                                var values = imageRecord.GetValues<string>(DicomTag.ReferencedFileID);
                                var relativePath = string.Join(@"\", values);

                                series.SopClassUid = imageRecord.GetSingleValueOrDefault(DicomTag.ReferencedSOPClassUIDInFile, "");
                                series.NumberOfImages += Math.Max(1, imageRecord.GetSingleValueOrDefault(DicomTag.NumberOfFrames, 1));
                                var absolutePath = Path.GetDirectoryName(path) + @"\\" + relativePath;
                                series.FileNames.Add(absolutePath);
                            }

                            if (generateMissingThumbnails && series.Thumbnail == null && series.FileNames.Count > 0)
                            {
                                GenerateThumbnail(series);
                            }
                            if (series.NumberOfImages > 0)
                            {
                                result.Add(series);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Could not read DICOMDIR file '{e.Message}' {e.StackTrace}");
            }
            return result;
        }

        public static IEnumerable<Series> ExtractSeriesFromSingleFile(string path)
        {
            var seriesList = new List<DicomSeries>();
            var dicomFile = DicomFile.Open(path, FileReadOption.ReadLargeOnDemand);
            ProcessFile(dicomFile, seriesList);
            PostProcessSeries(seriesList);
            return seriesList;
        }

        public static IEnumerable<Series> ExtractSeriesFromDirectory(string path)
        {
            var seriesList = new List<DicomSeries>();
            var files = Directory.EnumerateFiles(path);
            var dicomFiles = files.Where(file => DicomFile.HasValidHeader(file));
            foreach (var file in dicomFiles)
            {
                var dicomFile = DicomFile.Open(file, FileReadOption.ReadLargeOnDemand);
                ProcessFile(dicomFile, seriesList);
            }
            PostProcessSeries(seriesList);

            return seriesList;
        }

        public static void ProcessFile(DicomFile file, List<DicomSeries> seriesList)
        {
            string seriesInstanceUid = file.Dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
            if (string.IsNullOrEmpty(seriesInstanceUid)) { return; }

            CreateSeries(file, seriesInstanceUid, seriesList);
        }

        private static void CreateSeries(DicomFile file, string seriesInstanceUid, List<DicomSeries> seriesList)
        {
            var sopclass = file.Dataset.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);
            if (!IsSupportedSopClass(sopclass))
            {
                logger.LogError($"SOP class not supported: '{sopclass}' ");
                return;
            }

            var series = seriesList.FirstOrDefault(s => s.SeriesInstanceUid == seriesInstanceUid);
            if (series == null)
            {
                var seriesNumber = file.Dataset.GetValueOrDefault(DicomTag.SeriesNumber, 0, string.Empty);

                series = new DicomSeries
                {
                    SeriesInstanceUid = seriesInstanceUid,
                    SopClassUid = sopclass,
                    Number = seriesNumber
                };
                seriesList.Add(series);
            }
            series.FileNames.Add(file.File.Name);
            series.Files.Add(file);
        }

        public static bool IsSupportedSopClass(string sopClass)
        {
            return
                sopClass == DicomSopClasses.CTImageStorageSopClass ||
                sopClass == DicomSopClasses.EnhancedMRImageStorageSopClass ||
                sopClass == DicomSopClasses.MRImageStorageSopClass ||
                sopClass == DicomSopClasses.XA3DImageStorageSopClass ||
                sopClass == DicomSopClasses.XRayAngiographicImageStorageSopClass;
        }

        public static bool Is3DCapableSopClass(string sopClass)
        {
            bool mr3d =
                sopClass == DicomSopClasses.MRImageStorageSopClass ||
                sopClass == DicomSopClasses.EnhancedMRImageStorageSopClass;

            return
                sopClass == DicomSopClasses.XA3DImageStorageSopClass ||
                sopClass == DicomSopClasses.CTImageStorageSopClass ||
                (mr3d);
        }

        private static void GenerateThumbnail(DicomSeries series)
        {
            for (int i = series.FileNames.Count / 2; i >= 0; i--)
            {
                var middleFileName = series.FileNames[i];
                var middleFile = DicomFile.Open(middleFileName);
                if (middleFile.Dataset.Contains(DicomTag.PixelData))
                {
                    DicomImage image = new DicomImage(middleFile.Dataset);
                    image.Scale = 0.25;
                    var frame = image.NumberOfFrames / 2;
                    var renderedImage = image.RenderImage(frame);
                    series.Thumbnail = renderedImage.AsWriteableBitmap();
                    break;
                }
            }
        }

        private static void PostProcessSeries(List<DicomSeries> seriesList)
        {
            foreach (var series in seriesList)
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
                var sopclass = firstFile.Dataset.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);
                series.Is3D = Is3DCapableSopClass(sopclass) && (series.FileNames.Count > 1 || nrFrames > 1);

                GenerateThumbnail(series);
            }
        }
    }
}
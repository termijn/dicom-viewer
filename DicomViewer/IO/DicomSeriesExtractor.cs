using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dicom;
using Entities;

namespace DicomViewer.IO
{
    public class DicomSeriesExtractor
    {
        private List<DicomSeries3D> _series3D;

        public DicomSeriesExtractor()
        {

        }

        public IEnumerable<Series3D> ExtractSeries3D(string path)
        {
            var files = Directory.EnumerateFiles(path);
            var dicomFiles = files.Where(file => DicomFile.HasValidHeader(file));
            foreach(var file in dicomFiles)
            {
                ProcessFile(DicomFile.Open(file, FileReadOption.ReadLargeOnDemand));
            }
            return _series3D;
        }

        public void ProcessFile(DicomFile file)
        {
            string seriesInstanceUid = file.Dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
            if (string.IsNullOrEmpty(seriesInstanceUid)) return;

            if (Is3D(file))
            {
                CreateSeries3D(file, seriesInstanceUid);
            }
            else
            {

            }
        }

        private void CreateSeries3D(DicomFile file, string seriesInstanceUid)
        {
            var series = _series3D.FirstOrDefault(s => s.SeriesInstanceUid == seriesInstanceUid);
            if (series == null)
            {
                series = new DicomSeries3D
                {
                    SeriesInstanceUid = seriesInstanceUid,
                };
                _series3D.Add(series);
            }
            series.Files.Add(file);
        }

        public bool Is3D(DicomFile file)
        {
            var sopclass = file.Dataset.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);

            bool mr3d = 
                sopclass == DicomSopClasses.MRImageStorageSopClass || 
                sopclass == DicomSopClasses.EnhancedMRImageStorageSopClass;
            var isVolumetric = file.Dataset.GetValueOrDefault(DicomTag.VolumetricProperties, 0, string.Empty) == "VOLUME";

            return
                sopclass == DicomSopClasses.XA3DImageStorageSopClass ||
                sopclass == DicomSopClasses.CTImageStorageSopClass ||
                (mr3d && isVolumetric);
        }
    }
}

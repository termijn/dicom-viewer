using Dicom;
using Entities;
using System.Collections.Generic;

namespace DicomViewer.IO
{
    public class DicomSeries: Series
    {
        public List<DicomFile> Files { get; set; } = new List<DicomFile>();
        public List<string> FileNames { get; set; } = new List<string>();

        public void OpenFiles()
        {
            Files.Clear();
            foreach(var path in FileNames)
            {
                Files.Add(DicomFile.Open(path));
            }
        }
    }
}

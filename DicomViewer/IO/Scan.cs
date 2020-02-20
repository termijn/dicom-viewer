using Entities;

namespace DicomViewer.IO
{
    public class Scan
    {
        public Patient Patient { get; set; }
        public VolumeData Volume { get; set; }
    }
}

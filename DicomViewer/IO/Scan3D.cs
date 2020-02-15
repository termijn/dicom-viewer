using Entities;

namespace DicomViewer.IO
{
    public class Scan3D
    {
        public Patient Patient { get; set; }
        public VolumeData Volume { get; set; }
    }
}

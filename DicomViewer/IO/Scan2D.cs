using Entities;
using System.Collections.Generic;

namespace DicomViewer.IO
{
    public class Scan2D
    {
        public Patient Patient { get; set; }
        public List<ImageData> Images { get; set; } = new List<ImageData>();
    }
}

using System;
using System.Windows.Media;

namespace Entities
{
    public class Series
    {
        public string SeriesInstanceUid { get; set; }
        public string SopClassUid { get; set; }
        public string Number { get; set; }
        public int NumberOfImages { get; set; }
        public DateTime Time { get; set; }       
        public bool Is3D { get; set; }
        public ImageSource Thumbnail { get; set; }
    }
}

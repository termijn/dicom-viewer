using Dicom;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewer.IO
{
    public class DicomSeries3D: Series3D
    {
        public List<DicomFile> Files { get; set; } = new List<DicomFile>();
    }
}

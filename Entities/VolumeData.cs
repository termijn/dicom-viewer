using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class VolumeData
    {
        public List<ImageData> Slices { get; set; } = new List<ImageData>();

        /// <summary>
        /// Gets the dimensions of the volume
        /// x => width of a single slice in pixels
        /// y => number of slices in pixels
        /// z => height of a single slice in pixels
        /// </summary>
        public Dimensions3 Dimensions { get; set; } = new Dimensions3();
        public Matrix TransformationToPatient = new Matrix();

        public Vector3 CenterInVoxelSpace
        {
            get
            {
                var size = Dimensions.AsVector();
                return size / 2;
            }
        }

        public Vector3 CenterInPatientSpace
        {
            get
            {
                var firstSlice = Slices.First();
                var lastSlice = Slices.Last();
                var centerSlice0 = firstSlice.CenterPatient;
                var centerSlice1 = lastSlice.CenterPatient;
                return 0.5 * (centerSlice0 + centerSlice1);
            }
        }

        public Spacing3 VoxelSpacing { get; set; } = new Spacing3();

        public double RescaleIntercept { get; set; }
        public double RescaleSlope { get; set; }

        public bool IsSigned
        {
            get
            {
                return Slices.First().IsSigned;
            }
        }

        public IEnumerable<IntPtr> GetSlicePointers()
        {
            return Slices.Select(i => i.Pixels);
        }
    }
}

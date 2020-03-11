using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities
{
    public class ImageSet
    {
        public List<ImageData> Slices { get; set; } = new List<ImageData>();

        /// <summary>
        /// Gets the dimensions of the volume
        /// x => width of a single slice in pixels
        /// y => number of slices in pixels
        /// z => height of a single slice in pixels
        /// </summary>
        public Dimensions3 Dimensions { get; set; } = new Dimensions3();

        public Vector3 NumberOfVoxelsInPatientSpace
        {
            get
            {
                var voxelEdge = new Vector3(Dimensions.X, Dimensions.Y, Dimensions.Z);
                var edgeInPatientSpace = voxelEdge * TransformationToPatient;
                return new Vector3(Math.Abs(edgeInPatientSpace.X), Math.Abs(edgeInPatientSpace.Y), Math.Abs(edgeInPatientSpace.Z));
            }
        }

        public Vector3 DimensionsInPatientSpace
        {
            get
            {
                var voxelEdge = new Vector3(Dimensions.X * VoxelSpacing.X, Dimensions.Y * VoxelSpacing.Y, Dimensions.Z * VoxelSpacing.Z);
                var edgeInPatientSpace = voxelEdge * TransformationToPatient;
                return new Vector3(Math.Abs(edgeInPatientSpace.X), Math.Abs(edgeInPatientSpace.Y), Math.Abs(edgeInPatientSpace.Z));
            }
        }

        public Matrix TransformationToPatient { get; set; } = new Matrix();

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

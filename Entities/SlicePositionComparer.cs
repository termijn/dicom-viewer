using System.Collections.Generic;

namespace Entities
{
    /// <summary>
    /// Compares two slices according to their position along the given axis in patient space.
    /// </summary>
    public class SlicePositionComparer : IComparer<ImageData>
    {
        private readonly Vector3 _axis;

        public SlicePositionComparer(Vector3 axis)
        {
            _axis = axis.Normalized();
        }

        public int Compare(ImageData x, ImageData y)
        {
            Vector3 v0 = x.PositionPatient;
            Vector3 v1 = y.PositionPatient;

            Vector3 delta = (v1 - v0).Normalized();
            var dot = delta.Dot(_axis);
            if (Double.EqualsZero(dot)) // slices run orthogonal along axis
            {
                return 0;
            }
            if (Double.GreaterZero(dot)) // slices are sorted along axis (i0 < i1)
            {
                return -1;
            }
            else // slices are sorted inverse to the axis (i1 < i0)
            {
                return 1;
            }

        }
    }
}

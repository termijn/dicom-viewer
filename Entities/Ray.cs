using System;

namespace Entities
{

    public class Ray
    {
        private readonly Vector3 _origin;
        private readonly Vector3 _direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            _origin = origin;
            _direction = direction;
        }

        public double PointToRayOffset(Vector3 point)
        {
            double l = _direction.SqrLength();
            if (Double.EqualsZero(l)) { return double.NaN; }

            Vector3 pos = point - _origin;
            return _direction.Dot(pos) / l;
        }

        public BoundVector PointToRay(Vector3 point)
        {
            double factor = PointToRayOffset(point);
            if (double.IsNaN(factor)) { return new BoundVector(new Vector3(0, 0, 0), new Vector3(0, 0, 0)); }

            Vector3 intersect = _origin + _direction * factor;
            return new BoundVector(point, intersect);
        }

        public BoundVector Intersect(Ray other)
        {
            Vector3 origin1 = _origin;
            Vector3 origin2 = other._origin;
            Vector3 vec1 = _direction.Normalized();
            Vector3 vec2 = other._direction.Normalized();

            Vector3 normalA = vec1.Cross(vec2);
            double sqrLength = normalA.SqrLength();
            if (Double.EqualsZero(sqrLength))
            {
                // Parallel
                return  other.PointToRay(_origin);
            }

            normalA /= Math.Sqrt(sqrLength);
            Vector3 normalB = vec1.Cross(normalA);

            double divider2 = normalB.Dot(vec2);
            double t2 = (normalB.Dot(origin1) - normalB.Dot(origin2)) / divider2;
            Vector3 intersect2 = t2 * vec2 + origin2;

            double divider1 = normalA.Dot(normalA);
            double t1 = (normalA.Dot(origin1) - normalA.Dot(intersect2)) / divider1;
            Vector3 intersect1 = t1 * normalA + intersect2;

            return new BoundVector(intersect2, intersect1);
        }
    }
}

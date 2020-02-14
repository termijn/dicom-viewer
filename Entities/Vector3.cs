using System;

namespace Entities
{
    public class Vector3
    {
        public Vector3()
        {

        }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3 Normalized()
        {
            double l = Length();
            if (Double.EqualsZero(0)) { return new Vector3(X, Y, Z); }

            double factor = 1.0 / l;
            return new Vector3
            {
                X = X * factor,
                Y = Y * factor,
                Z = Z * factor
            };
        }

        public double SqrLength()
        {
            return Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2);
        }

        public double Length()
        {
            return Math.Sqrt(SqrLength());
        }

        public Vector3 Cross(Vector3 v)
        {
            return new Vector3 {
                X = Y * v.Z - Z * v.Y,
                Y = Z * v.X - X * v.Z,
                Z = X * v.Y - Y * v.X
            };
        }

        public double Dot(Vector3 v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public static Vector3 operator *(Vector3 v, double scalar)
        {
            return new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3 operator /(Vector3 v, double scalar)
        {
            return new Vector3(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }

        public static Vector3 operator *(double scalar, Vector3 v)
        {
            return new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3 operator /(double scalar, Vector3 v)
        {
            return new Vector3(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }

        public static Vector3 operator /(Vector3 v1, Vector3 v2)
        { return new Vector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z); }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            var result = new Vector3
            {
                X = v1.X - v2.X,
                Y = v1.Y - v2.Y,
                Z = v1.Z - v2.Z
            };
            return result;
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            var result = new Vector3
            {
                X = v1.X + v2.X,
                Y = v1.Y + v2.Y,
                Z = v1.Z + v2.Z
            };
            return result;
        }
        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            var result = new Vector3
            {
                X = v1.X * v2.X,
                Y = v1.Y * v2.Y,
                Z = v1.Z * v2.Z
            };
            return result;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}

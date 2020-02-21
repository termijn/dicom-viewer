using System;

namespace Entities
{
    public class Matrix
    {
        private const int N = 4;
        public double[] Elements { get; } = new double[N * N];

        public Matrix()
        {
            Elements[0] = 1;
            Elements[5] = 1;
            Elements[10] = 1;
            Elements[15] = 1;
        }

        public double this[int i]
        {
            get { return Elements[i]; }
            set { Elements[i] = value; }
        }

        public double this[int x, int y]
        {
            get { return Elements[y * N + x]; }
            set { Elements[y * N + x] = value; }
        }

        public static Matrix Translation(Vector3 translationVector)
        {
            var result = new Matrix();
            result[0, N - 1] = translationVector.X;
            result[1, N - 1] = translationVector.Y;
            result[2, N - 1] = translationVector.Z;
            return result;
        }
        
        public static Matrix Scaling(Vector3 scale)
        {
            var scaling = new Matrix();
            scaling[0, 0] = scale.X;
            scaling[1, 1] = scale.Y;
            scaling[2, 2] = scale.Z;
            return scaling;
        }
      
        public static Matrix RotationAngleAxis(double angle, Vector3 vec)
        {
            return RotationAxis(Math.Cos(angle), Math.Sin(angle), vec);
        }
        
        public static Matrix RotationAxis(double cosAngle, double sinAngle, Vector3 axis)
        {
            var rotation = new Matrix();

            if (Double.EqualsZero(axis.SqrLength())) { return  new Matrix(); }
            axis = axis.Normalized();

            double t = 1.0 - cosAngle;
            rotation[0, 0] = cosAngle + axis.X * axis.X * t;
            rotation[1, 1] = cosAngle + axis.Y * axis.Y * t;
            rotation[2, 2] = cosAngle + axis.Z * axis.Z * t;

            double tmp1 = axis.X * axis.Y * t;
            double tmp2 = axis.Z * sinAngle;
            rotation[1, 0] = tmp1 + tmp2;
            rotation[0, 1] = tmp1 - tmp2;

            tmp1 = axis.X * axis.Z * t;
            tmp2 = axis.Y * sinAngle;
            rotation[2, 0] = tmp1 - tmp2;
            rotation[0, 2] = tmp1 + tmp2;

            tmp1 = axis.Y * axis.Z * t;
            tmp2 = axis.X * sinAngle;
            rotation[2, 1] = tmp1 + tmp2;
            rotation[1, 2] = tmp1 - tmp2;

            return rotation;
        }

        public static Matrix LookAt(Vector3 from, Vector3 to, Vector3 up)
        {
            // Viewing direction is along negative z-axis of camera
            Vector3 zaxis = -(to - from).Normalized();
            Vector3 xaxis = up.Cross(zaxis).Normalized();
            Vector3 yaxis = zaxis.Cross(xaxis);

            var matrix = new Matrix();
            matrix.SetColumn(0, xaxis.X, xaxis.Y, xaxis.Z, 0);
            matrix.SetColumn(1, yaxis.X, yaxis.Y, yaxis.Z, 0);
            matrix.SetColumn(2, zaxis.X, zaxis.Y, zaxis.Z, 0);

            return matrix * Translation(-from);
        }

        public Vector3 Translation()
        {            
            return new Vector3(this[0, 3], this[1, 3], this[2, 3]);
        }

        public void SetColumn(int column, double x, double y, double z, double w)
        {
            this[0, column] = x;
            this[1, column] = y;
            this[2, column] = z;
            this[3, column] = w;
        }

        private static double Square(double a) { return a * a; }

        public Matrix Inverted()
        {
            var start = new Matrix();
            var end = Clone();

            int diagonal;
            for (diagonal = 0; diagonal < N; ++diagonal)
            {
                int c;
                int maxC = diagonal;
                double maxVal = Math.Abs(end[diagonal, diagonal]);
                for (c = diagonal + 1; c < N; ++c)
                {
                    double temp = Math.Abs(end[diagonal, c]);
                    if (temp > maxVal)
                    {
                        maxVal = temp;
                        maxC = c;
                    }
                }

                if (!Double.GreaterZero(maxVal)) { throw new InvalidOperationException("axMatrix::Inverted(): Inverse matrix does not exist!"); }
                if (maxC != diagonal)
                {
                    end.SwapColumns(diagonal, maxC);
                    start.SwapColumns(diagonal, maxC);
                }

                // Transform all non-diagonal value to zero (pivoting)
                for (c = 0; c < N; ++c)
                {
                    if (c != diagonal && !Double.EqualsZero(end[diagonal, c]))
                    {
                        double factor = -end[diagonal, c] / end[diagonal, diagonal];
                        end.MultiplyAddColumns(c, diagonal, factor);
                        start.MultiplyAddColumns(c, diagonal, factor);
                    }
                }
            }
            
            for (diagonal = 0; diagonal < N; ++diagonal)
            {
                if (Double.EqualsZero(end[diagonal, diagonal])) { throw new InvalidOperationException("No inverse matrix possible"); }
                double factor = 1.0 / end[diagonal, diagonal];
                start.MultiplyColumn(diagonal, factor);
            }

            return start;
        }

        public void MultiplyAddColumns(int c1, int c2, double factor)
        {
            for (int i = 0; i < N; ++i) { this[i, c1] += factor * this[i, c2]; }
        }

        public void MultiplyColumn(int c, double factor) 
        { for (int i = 0; i < N; ++i) { this[i, c] *= factor; } }


        public void SwapColumns(int c1, int c2)
        {
            for (int n = 0; n < N; ++n)
            {
                double t = this[n, c1];
                this[n, c1] = this[n, c2];
                this[n, c2] = t;
            }
        }

        public AngleAxis GetAngleAxis()
        {
            var result = new AngleAxis();
            double norm = 
                Math.Sqrt(Square(this[2, 1] - this[1, 2]) + Square(this[0, 2] - this[2, 0]) + Square(this[1, 0] - this[0, 1]));

            if (!Double.EqualsZero(norm)) { norm = 1.0 / norm; }
            result.Angle = Math.Acos(0.5 * (this[0, 0] + this[1, 1] + this[2, 2] - 1.0));
            result.Axis.X = (this[2, 1] - this[1, 2]) * norm;
            result.Axis.Y = (this[0, 2] - this[2, 0]) * norm;
            result.Axis.Z = (this[1, 0] - this[0, 1]) * norm;
            return result;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            var result = new Matrix();
            int i = 0;
            for (int c = 0; c < N; ++c)
            {
                for (int r = 0; r < N; ++r, ++i)
                {
                    result[i] = 0.0;
                    for (int n = 0; n < N; ++n) { result[i] += m1[r, n] * m2[n, c]; }
                }
            }

            return result;
        }


        public static Vector3 operator *(Matrix m, Vector3 v)
        {
            double w = v.X * m[3] + v.Y * m[7] + v.Z * m[11] + m[15];
            if (Double.EqualsZero(w))
            {
                w = 1.0;
            }
            else { w = 1.0 / w; }
            return new Vector3 {
                X = (v.X * m[0] + v.Y * m[4] + v.Z * m[8] + m[12]) * w,
                Y = (v.X * m[1] + v.Y * m[5] + v.Z * m[9] + m[13]) * w,
                Z = (v.X * m[2] + v.Y * m[6] + v.Z * m[10] + m[14]) * w
            };
        }

        public OrientationXYZ GetAnglesZYX()
        {
            var result = new OrientationXYZ();

            result.Y = -Math.Asin(this[2, 0]);  //or ay = pi - arcsin(At(2,0)), both eventually deliver the same rotation matrix
            double cosAy = Math.Cos(result.Y);
            if (Double.EqualsZero(cosAy))  //ay = +pi/2 or ay= -pi/2
            {
                result.X = 0.0;  //set ax to zero, so it does not matter whether ay = +pi/2 or ay= -pi/2
                result.Z = -Math.Asin(this[0, 1]);
                double cosAz = this[1, 1];
                if (cosAz < 0.0) { result.Z = Math.PI - result.Z; }
            }
            else
            {
                double divCosAy = 1.0 / cosAy;

                result.X = Math.Asin(this[2, 1] * divCosAy);
                double cosAx = this[2, 2] * divCosAy;
                if (cosAx < 0.0) { result.X = Math.PI - result.X; }

                result.Z = Math.Asin(this[1, 0] * divCosAy);
                double cosAz = this[0, 0] * divCosAy;
                if (cosAz < 0.0) { result.Z = Math.PI - result.Z; }
            }
            return result;
        }

        public static Vector3 operator *(Vector3 v, Matrix m)
        {
            double w = v.X * m[3] + v.Y * m[7] + v.Z * m[11] + m[15];
            if (Double.EqualsZero(w))
            {
                w = 1.0;
            }
            else { w = 1.0 / w; }
            return new Vector3
            {
                X = (v.X * m[0] + v.Y * m[4] + v.Z * m[8] + m[12]) * w,
                Y = (v.X * m[1] + v.Y * m[5] + v.Z * m[9] + m[13]) * w,
                Z = (v.X * m[2] + v.Y * m[6] + v.Z * m[10] + m[14]) * w
            };
        }

        public Matrix Clone()
        {
            var result = new Matrix();
            Array.Copy(Elements, result.Elements, Elements.Length);
            return result;
        }
    }
}

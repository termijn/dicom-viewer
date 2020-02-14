using System;

namespace Entities
{
    public static class Double
    {
        public static bool EqualsZero(double value)
        {
            return Math.Abs(value) < 0.00001;
        }

        public static bool GreaterZero(double value)
        {
            return value > 0.00001;
        }

        public static double DegreesToRadians(double degrees)
        {
            return ((Math.PI * 2) / 360) * degrees;
        }
    }
}

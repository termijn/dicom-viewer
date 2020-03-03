namespace Entities
{
    public class Spacing3
    {
        public double X { get; set; } = 1;
        public double Y { get; set; } = 1;
        public double Z { get; set; } = 1;

        public Vector3 AsVector()
        {
            return new Vector3(X, Y, Z);
        }
    }
}

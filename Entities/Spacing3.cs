namespace Entities
{
    public class Spacing3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3 AsVector()
        {
            return new Vector3(X, Y, Z);
        }
    }
}

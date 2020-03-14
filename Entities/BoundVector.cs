namespace Entities
{
    public class BoundVector
    {
        public BoundVector()
        {

        }

        public BoundVector(Vector3 from, Vector3 to)
        {
            From = from;
            To = to;
        }

        public Vector3 From { get;set; }
        public Vector3 To { get; set; }

        public double Length()
        {
            return (To - From).Length();
        }
    }
}

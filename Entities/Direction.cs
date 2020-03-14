namespace Entities
{
    public class Direction
    {
        private readonly Space space;

        public Direction(Space space, Vector3 direction)
        {
            this.space = space;

            From = new Coordinate(space, new Vector3(0, 0, 0));
            To = new Coordinate(space, direction);
        }

        public Direction(Space space, Coordinate origin, Coordinate target)
        {
            this.space = space;
            From = origin.To(space);
            To = origin.To(space);
        }

        public Coordinate From { get; }
        public Coordinate To { get; }

        public Vector3 In(Space targetSpace)
        {
            var originInTargetSpace = From.In(targetSpace);
            var targetInTargetSpace = To.In(targetSpace);
            return targetInTargetSpace - originInTargetSpace;
        }
    }
}
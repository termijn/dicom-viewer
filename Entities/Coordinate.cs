namespace Entities
{

    public class Coordinate
    {
        private readonly Space _space;
        private readonly Vector3 _vector;

        public Coordinate(Space space, Vector3 vector)
        {
            _space = space;
            _vector = vector;
        }

        public Vector3 In(Space targetSpace)
        {
            var transform = _space.TransformationTo(targetSpace);
            return _vector * transform;
        }

        public Coordinate To(Space targetSpace)
        {
            var vectorInTargetSpace = In(targetSpace);
            return new Coordinate(targetSpace, vectorInTargetSpace);
        }
    }
}
using System;

namespace Entities
{
    public class Space
    {
        private Matrix _transformationToParent = new Matrix();

        public Space()
        {
        }

        public Space(Space parent)
        {
            Parent = parent;
        }

        public Coordinate Origin
        {
            get
            {
                return new Coordinate(this, new Vector3());
            }
        }

        public Direction AxisX => new Direction(this, new Vector3(1, 0, 0));
        public Direction AxisY => new Direction(this, new Vector3(0, 1, 0));
        public Direction AxisZ => new Direction(this, new Vector3(0, 0, 1));

        public event Action Changed;

        public Matrix TransformationToParent
        {
            get => _transformationToParent;
            set
            {
                if (!_transformationToParent.Equals(value))
                {
                    _transformationToParent = value;
                    var root = GetRoot();
                    root.Changed?.Invoke();
                }
            }
        }

        public Space Parent { get; }

        public Matrix TransformationTo(Space toSpace)
        {
            var thisRoot = GetRoot();
            var toRoot = toSpace.GetRoot();
            if (thisRoot != toRoot) { throw new ArgumentException("Spaces should be in the same tree"); }

            var transformToRoot = GetTransformationToRoot();
            var toTransformToRoot = toSpace.GetTransformationToRoot();

            return toTransformToRoot.Inverted() * transformToRoot;
        }

        public Matrix GetTransformationToRoot()
        {
            var result = new Matrix();
            var space = this;
            while (space != null)
            {
                result = space.TransformationToParent * result;
                space = space.Parent;
            }
            return result;
        }

        public Space GetRoot()
        {
            var space = this;
            while (space.Parent != null)
            {
                space = space.Parent;
            }
            return space;
        }
    }
}
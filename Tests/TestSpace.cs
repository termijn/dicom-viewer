using Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TestSpace
    {
        [TestMethod]
        public void TransformWithIdentityReturnsSameVector()
        {
            var root = new Space();
            var child = new Space(root);

            var coordinate = new Coordinate(child, new Vector3(10, 10, 10));
            var result = coordinate.In(root);
            Assert.AreEqual(new Vector3(10, 10, 10), result);
        }

        [TestMethod]
        public void TransformWithInverseReturns0()
        {
            var root = new Space();
            var child = new Space(root)
            {
                TransformationToParent = Matrix.Translation(-10, -10, -10)
            };

            var coordinate = new Coordinate(child, new Vector3(10, 10, 10));
            var result = coordinate.In(root);
            Assert.AreEqual(new Vector3(0, 0, 0), result);
        }

        [TestMethod]
        public void TransformToOtherBranch()
        {
            var root = new Space();
            var child1 = new Space(root);
            var child2 = new Space(root);

            child1.TransformationToParent = Matrix.Translation(10, 10, 10);
            child2.TransformationToParent = Matrix.Translation(20, 20, 20);

            var coordinate = new Coordinate(child1, new Vector3(0, 0, 0));
            var result = coordinate.In(child2);

            Assert.AreEqual(new Vector3(-10, -10, -10), result);
        }
    }
}

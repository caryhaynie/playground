using NUnit.Framework;
using PlayfulSoftware.Splines.Hybrid;
using UnityEngine;

namespace BezierTests
{
    public sealed class GetPoint3Tests
    {
        private Vector3 p0;
        private Vector3 p1;
        private Vector3 p2;

        [SetUp]
        public void Setup()
        {
            p0 = new Vector3(1f, 0f, 0f);
            p1 = new Vector3(2f, 1f, 0f);
            p2 = new Vector3(3f, 0f, 0f);
        }

        [Test]
        public void ReturnsP0WhenTIsZero()
        {
            var p = Bezier.GetPoint(p0, p1, p2, 0f);
            Assert.That(p, Is.EqualTo(p0));
        }

        [Test]
        public void ReturnsP2WhenTIsOne()
        {
            var p = Bezier.GetPoint(p0, p1, p2, 1f);
            Assert.That(p, Is.EqualTo(p2));
        }
    }
}
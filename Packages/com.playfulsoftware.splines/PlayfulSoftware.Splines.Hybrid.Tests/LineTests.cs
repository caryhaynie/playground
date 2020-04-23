using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using PlayfulSoftware.Splines.Hybrid;

namespace LineTests
{
    public sealed class InitializationTests
    {
        /*
        public sealed class LineTestBehaviour : MonoBehaviour, IMonoBehaviourTest
        {
            public bool IsTestFinished => true;
        }

        [UnityTest]
        public IEnumerator LineBehaviorWorks()
        {
            yield return new MonoBehaviourTest<LineTestBehaviour>();
        }
        */

        [Test]
        public void P0IsInitializedToZero()
        {
            var go = new GameObject();
            var line = go.AddComponent<Line>();

            Assert.That(line.p0, Is.EqualTo(Vector3.zero));

            GameObject.Destroy(go);
        }

        [Test]
        public void P1IsInitializedToZero()
        {
            var go = new GameObject();
            var line = go.AddComponent<Line>();

            Assert.That(line.p1, Is.EqualTo(Vector3.zero));

            GameObject.Destroy(go);
        }
    }
}
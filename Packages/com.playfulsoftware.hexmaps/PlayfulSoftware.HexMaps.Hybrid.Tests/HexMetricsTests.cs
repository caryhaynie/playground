using System.Collections;
using NUnit.Framework;
using PlayfulSoftware.HexMaps.Hybrid;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif // UNITY_EDITOR

namespace Tests
{
    public class HexMetricsTests
    {
        private const string kTestSceneName = "TestData/Scenes/HexMetricsTestScene";

        // A Test behaves as an ordinary method
        [Test]
        public void HexMetricsTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator HexMetricsTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [Test]
        public void HexMetricsThowsExceptionsWhenNotInitialized()
        {
            Assert.That(HexMetrics.initialized, Is.False);
            Assert.That(() => HexMetrics.outerRadius, Throws.Exception);
        }

        [UnityTest]
        public IEnumerator HexMetricsDoesNotThrowWhenInitialized()
        {
            Assert.That(HexMetrics.initialized, Is.False);
            yield return LoadSceneAsync(kTestSceneName);
            yield return null;
            Assert.That(HexMetrics.initialized, Is.True);
            HexMetrics.parametersAsset = null;
            yield return SceneManager.UnloadSceneAsync(kTestSceneName);
            Assert.That(HexMetrics.initialized, Is.False);
        }

        [UnityTest]
        public IEnumerator HexMetricsRadiusValuesAreConsistent()
        {
            yield return LoadSceneAsync(kTestSceneName);
            yield return null;

            var outerRadius = HexMetrics.outerRadius;
            var innerRadius = HexMetrics.innerRadius;
            var innerToOuter = HexMetrics.innerToOuter;
            var outerToInner = HexMetrics.outerToInner;

            Assert.That(outerRadius * outerToInner, Is.EqualTo(innerRadius).Using(FloatEqualityComparer.Instance));
            Assert.That(innerRadius * innerToOuter, Is.EqualTo(outerRadius).Using(FloatEqualityComparer.Instance));

            HexMetrics.parametersAsset = null;
            yield return SceneManager.UnloadSceneAsync(kTestSceneName);
        }

        private AsyncOperation LoadSceneAsync(string name)
        {
            var loadParams = new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.None
            };
#if UNITY_EDITOR
            var path = $"Assets/{name}.unity";
            return EditorSceneManager.LoadSceneAsyncInPlayMode(path, loadParams);
#else
            return SceneManager.LoadSceneAsync(name, loadParams);
#endif // UNITY_EDITOR

        }
    }
}

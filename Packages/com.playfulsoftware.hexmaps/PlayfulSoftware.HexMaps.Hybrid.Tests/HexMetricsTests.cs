using System.Collections;
using NUnit.Framework;
using PlayfulSoftware.HexMaps.Hybrid;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

#endif // UNITY_EDITOR

namespace Tests
{
    public class HexMetricsTests
    {
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
            yield return LoadSceneAsync("TestData/Scenes/HexMetricsTestScene");
            //yield return SceneManager.LoadSceneAsync("TestData/Scenes/HexMetricsTestScene", LoadSceneMode.Additive);
            yield return null;
            // var go = GameObject.Find("ParameterAssetLoader");
            // if (!go)
            //     Assert.Fail("Failed to find the loader gameobject!");
            // var loader = go.GetComponent<HexMapGenerationParametersTestLoader>();
            // if (!loader)
            //     Assert.Fail("Failed to find the loader component!");
            // HexMetrics.parametersAsset = loader.parametersAsset;
            Assert.That(HexMetrics.initialized, Is.True);
            HexMetrics.parametersAsset = null;
            yield return SceneManager.UnloadSceneAsync("TestData/Scenes/HexMetricsTestScene");
            Assert.That(HexMetrics.initialized, Is.False);
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

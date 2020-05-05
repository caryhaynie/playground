using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PlayfulSoftware.HexMaps.Hybrid;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HexMapGenerationParametersTests
    {

        private const string ParameterTestObjectName = "TestHexMapGenerationParameters.asset";

        private void WithPathAndAsset(Action<string, HexMapGenerationParameters> test)
        {
            using (var path = new TemporaryAssetPath("TestData"))
            {
                var assetPath = Path.Combine(path.path, ParameterTestObjectName);
                using (var assetScope = new TemporaryAssetScope<HexMapGenerationParameters>(assetPath))
                {
                    // ensure OnValidate has been called.
                    assetScope.@object.OnValidate();
                    test(assetScope.assetPath, assetScope.@object);
                }
            }
        }

        // A Test behaves as an ordinary method
        [Test]
        public void HexMapGenerationParametersTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator HexMapGenerationParametersTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [Test]
        public void CreatingAssetWorks()
        {
            WithPathAndAsset((p, a) =>
            {
                Assert.That(AssetDatabase.Contains(a), Is.True);
            });

            Assert.That($"Assets/TestData/{ParameterTestObjectName}", Does.Not.Exist);
        }

        [Test]
        public void EnsureCornerArrayIsProperlyInitialized()
        {
            WithPathAndAsset((_, asset) =>
            {
                var corners = asset.GetCorners();
                Assert.That(corners, Is.Not.Null);
                Assert.That(corners, Is.Not.Empty);
                Assert.That(corners.Length, Is.EqualTo(6));
            });
        }

        [Test]
        public void EnsureHashGridIsProperlyInitialized()
        {
            WithPathAndAsset((_, asset) =>
            {
                var grid = asset.GetHashGrid();
                Assert.That(grid, Is.Not.Null);
            });
        }
    }
}

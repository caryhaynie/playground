using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HexMapAssetEditorTests
    {
        private string assetRoot => Application.dataPath;

        private TemporaryAssetPath TemporaryAssetPathFor(string name)
            => new TemporaryAssetPath(Path.Combine(assetRoot, "Temp", name));

        // A Test behaves as an ordinary method
        [Test]
        public void HexMapAssetEditorTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator HexMapAssetEditorTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

    }
}

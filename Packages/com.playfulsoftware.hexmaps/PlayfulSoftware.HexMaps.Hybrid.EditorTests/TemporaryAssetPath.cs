using NUnit.Framework;
using System;
using System.IO;

using UnityEditor;

namespace Tests
{
    internal class TemporaryAssetPath : IDisposable
    {
        internal string m_DeleteRoot = null;
        internal string path { get; }

        internal TemporaryAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (Path.IsPathRooted(path))
                throw new ArgumentException("Cannot be a rooted path", "path");
            if (Directory.Exists(path))
                throw new InvalidOperationException("Requested temporary path already exists!");
            this.path = Path.Combine("Assets", path);
            FindFirstParentDirectoryThatActuallyExists();
            Directory.CreateDirectory(this.path);
        }

        private void FindFirstParentDirectoryThatActuallyExists()
        {
            var p = path;
            var parent = Path.GetDirectoryName(p);
            while (parent != "Assets" || !Directory.Exists(parent))
            {
                p = parent;
                parent = Path.GetDirectoryName(p);
            }
            m_DeleteRoot = p;
        }

        private string MetaFileFor(string path)
        {
            return string.Format("{0}.meta", path.TrimEnd(new[] { '\\', '/' }));
        }

        void IDisposable.Dispose()
        {
            Directory.Delete(m_DeleteRoot, true);
            File.Delete(MetaFileFor(m_DeleteRoot));
            AssetDatabase.Refresh();
        }
    }

    public class TemporaryAssetPathTests
    {
        [Test]
        public void ProperlyCleansUpIntermediatePaths()
        {
            string path = "Some/Test/Path";
            var root = Path.Combine("Assets", "Some");
            var root_meta = Path.Combine("Assets", "Some.meta");
            using (new TemporaryAssetPath(path)) { }
            Assert.That(root, Does.Not.Exist);
            Assert.That(root_meta, Does.Not.Exist);
        }

        [Test]
        public void FindsCorrectDeleteRoot()
        {
            string path = "Some/Test/Path";
            var root = Path.Combine("Assets", "Some");
            using (var tp = new TemporaryAssetPath(path))
                Assert.That(tp.m_DeleteRoot, Is.EqualTo(root), () => string.Format("Expected: {0}, Actual: {1}", root, tp.m_DeleteRoot));
        }
    }
}
using System;
using UnityEditor;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Tests
{
    internal class TemporaryAssetScope<T> : IDisposable where T : ScriptableObject, new()
    {
        private readonly string m_AssetPath;
        private readonly T m_Object;

        public string assetPath => m_AssetPath;
        public T @object => m_Object;

        public TemporaryAssetScope(string assetPath)
        {
            m_AssetPath = assetPath;
            m_Object = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(m_Object, m_AssetPath);
            AssetDatabase.SaveAssets();
        }

        void IDisposable.Dispose()
        {
            AssetDatabase.DeleteAsset(m_AssetPath);
        }
    }
}
#if UNITY_EDITOR
using System;
using UnityEditor;

using UnityObject = UnityEngine.Object;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    sealed class SerializedObjectScope : IDisposable
    {
        public readonly SerializedObject serializedObject;

        public SerializedObjectScope(params UnityObject[] objects)
        {
            serializedObject = new SerializedObject(objects);
        }

        void IDisposable.Dispose() => serializedObject.ApplyModifiedProperties();
    }
}
#endif // UNITY_EDITOR
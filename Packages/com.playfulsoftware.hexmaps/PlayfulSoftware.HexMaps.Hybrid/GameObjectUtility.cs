using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    static class GameObjectUtility
    {
        public static void SafelyDeleteGameObject(GameObject go)
        {
            if (!go)
                throw new ArgumentNullException(nameof(go));
            if (Application.IsPlaying(go))
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }

        public static void SafelyDeleteUnityObject(Object obj, GameObject owner)
        {
            if (!obj)
                throw new ArgumentNullException(nameof(obj));
            if (!owner)
                throw new ArgumentNullException(nameof(owner));
            if (Application.IsPlaying(owner))
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}
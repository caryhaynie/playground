using System;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    [Serializable]
    public struct HexFeatureCollection
    {
        public Transform[] prefabs;

        public Transform Pick(float choice)
            => prefabs[(int) choice * prefabs.Length];
    }
}
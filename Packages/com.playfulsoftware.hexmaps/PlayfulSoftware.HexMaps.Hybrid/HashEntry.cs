using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public struct HashEntry
    {
        public float a;
        public float b;
        public float c;
        public float d;
        public float e;

        public static HashEntry Create()
            => new HashEntry {
                a = Random.value * 0.999f,
                b = Random.value * 0.999f,
                c = Random.value * 0.999f,
                d = Random.value * 0.999f,
                e = Random.value * 0.999f
            };
    }
}
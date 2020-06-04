using Unity.Entities;

namespace PlayfulSoftware.HexMaps
{
    public struct HexMapRules
    {
        public struct ChunkInfo : IComponentData
        {
            /// <summary>
            /// Width of a chunk, in cells.
            /// </summary>
            public int SizeX;
            /// <summary>
            /// Height of a chunk, in cells.
            /// </summary>
            public int SizeZ;
        }

        public struct EntropyInfo : IComponentData
        {
            public float CellPerturbStrength;
            public float ElevationPerturbStrength;
            public float NoiseScale;
            public int RandomSeed;
        }
        // These ratios are intrinsic to hexmaps, so they can't be changed.
        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;
    }
}
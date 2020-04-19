using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public static class HexMetrics
    {
        public const float blendFactor = 1f - solidFactor;
        public const float cellPerturbStrength = 4f;
        public const int chunkSizeX = 5, chunkSizeZ = 5;
        public const float elevationPerturbStrength = 1.5f;
        public const float elevationStep = 3f;
        public const float horizontalTerraceStepSize = 1f / terracedSteps;
        public const float innerRadius = outerRadius * outerToInner; // sqrt(3) / 2
        public const float noiseScale = 0.003f;
        public const float outerRadius = 10f;
        public const float waterSurfaceElevationOffset = -0.25f;
        public const float solidFactor = 0.8f;
        public const float streamBedElevationOffset = -1.75f;
        public const int terracesPerSlope = 2;
        public const int terracedSteps = terracesPerSlope * 2 + 1;
        public const uint triangleSubdivisions = 2;
        public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);
        public const float waterBlendFactor = 1f - waterFactor;
        public const float waterFactor = 0.6f;

        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;

        static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius), // N
            new Vector3(innerRadius, 0f,  0.5f * outerRadius), // NE
            new Vector3(innerRadius, 0f, -0.5f * outerRadius), // SE
            new Vector3(0f, 0f, -outerRadius), // S
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius), // SW
            new Vector3(-innerRadius, 0f,  0.5f * outerRadius) // NW
        };

        private static float[][] featureThresholds =
        {
            new float[] { 0f, 0f, 0.4f},
            new float[] { 0f, 0.4f, 0.6f},
            new float[] { 0.4f, 0.6f, 0.8f}
        };

        static HashGrid<Vector2> hashGrid;

        internal static Texture2D noiseSource;

        public static Vector3 GetBridge(HexDirection d)
            => (GetFirstCorner(d) + GetSecondCorner(d)) * blendFactor;

        public static Vector3 GetWaterBridge(HexDirection d)
            => (GetFirstCorner(d) + GetSecondCorner(d)) * waterBlendFactor;

        public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
        {
            var delta = Mathf.Abs(elevation2 - elevation1);
            switch (delta)
            {
                case 0: // same elevation
                    return HexEdgeType.Flat;
                case 1:
                    return HexEdgeType.Slope;
                default:
                    return HexEdgeType.Cliff;
            }
        }

        public static float[] GetFeatureThresholds(int level)
            => featureThresholds[level];

        public static Vector3 GetFirstCorner(HexDirection d)
            => corners[(int)d];

        public static Vector3 GetFirstSolidCorner(HexDirection d)
            => GetFirstCorner(d) * solidFactor;

        public static Vector3 GetFirstWaterCorner(HexDirection d)
            => GetFirstCorner(d) * waterFactor;

        public static Vector3 GetSecondCorner(HexDirection d)
            => corners[((int)d + 1) % corners.Length];

        public static Vector3 GetSecondSolidCorner(HexDirection d)
            => GetSecondCorner(d) * solidFactor;

        public static Vector3 GetSecondWaterCorner(HexDirection d)
            => GetSecondCorner(d) * waterFactor;

        public static Vector3 GetSolidEdgeMiddle(HexDirection d)
            => (corners[(int) d] + corners[((int)d + 1) % corners.Length])
                   * (0.5f * solidFactor);

        public static void InitializeHashGrid(int seed)
        {
            // save off current random state
            var oldState = Random.state;

            Random.InitState(seed);
            hashGrid = new HashGrid<Vector2>((_) => new Vector2(Random.value, Random.value), seed);
            // re-apply previous random state
            Random.state = oldState;
        }

        public static Vector3 Perturb(Vector3 position)
        {
            var sample = SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
            return position;
        }

        public static Vector2 SampleHashGrid(Vector3 position)
            => hashGrid?.Sample(position) ?? Vector2.zero;

        public static Vector4 SampleNoise(Vector3 position)
        {
            return noiseSource.GetPixelBilinear(
                position.x * noiseScale,
                position.z * noiseScale);
        }

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            var h = step * horizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;
            var v = ((step + 1) / 2) * verticalTerraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        }

        public static Color TerraceLerp(Color a, Color b, int step)
        {
            var h = step * horizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }
    }
}

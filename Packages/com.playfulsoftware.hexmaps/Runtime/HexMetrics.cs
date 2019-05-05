using UnityEngine;

namespace PlayfulSoftware.HexMaps
{
    public static class HexMetrics
    {
        public const float blendFactor = 1f - solidFactor;
        public const float cellPerturbStrength = 4f;
        public const float elevationPerturbStrength = 1.5f;
        public const float elevationStep = 3f;
        public const float horizontalTerraceStepSize = 1f / terracedSteps;
        public const float innerRadius = outerRadius * 0.866025404f; // sqrt(3) / 2
        public const float noiseScale = 0.003f;
        public const float outerRadius = 10f;
        public const float solidFactor = 0.8f;
        public const int terracesPerSlope = 2;
        public const int terracedSteps = terracesPerSlope * 2 + 1;
        public const uint triangleSubdivisions = 2;
        public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

        static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius), // N
            new Vector3(innerRadius, 0f,  0.5f * outerRadius), // NE
            new Vector3(innerRadius, 0f, -0.5f * outerRadius), // SE
            new Vector3(0f, 0f, -outerRadius), // S
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius), // SW
            new Vector3(-innerRadius, 0f,  0.5f * outerRadius) // NW
        };

        internal static Texture2D noiseSource;

        public static Vector3 GetBridge(HexDirection d)
        {
            return (GetFirstCorner(d) + GetSecondCorner(d)) * blendFactor;
        }

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

        public static Vector3 GetFirstCorner(HexDirection d)
        {
            return corners[(int)d];
        }

        public static Vector3 GetFirstSolidCorner(HexDirection d)
        {
            return GetFirstCorner(d) * solidFactor;
        }

        public static Vector3 GetSecondCorner(HexDirection d)
        {
            var i = ((int)d + 1) % (corners.Length);
            return corners[i];
        }

        public static Vector3 GetSecondSolidCorner(HexDirection d)
        {
            return GetSecondCorner(d) * solidFactor;
        }

        public static Vector4 SampleNoise(Vector3 position)
        {
            return noiseSource.GetPixelBilinear(
                position.x * noiseScale,
                position.z * noiseScale);
        }

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            float h = step * horizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;
            float v = ((step + 1) / 2) * verticalTerraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        }

        public static Color TerraceLerp(Color a, Color b, int step)
        {
            float h = step * horizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }
    }
}

using UnityEngine;

namespace FourEx
{
    public static class HexMetrics
    {
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * 0.866025404f; // sqrt(3) / 2
        public const float solidFactor = 0.75f;
        public const float blendFactor = 1f - solidFactor;
        public const float elevationStep = 5f;

        static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius), // N
            new Vector3(innerRadius, 0f,  0.5f * outerRadius), // NE
            new Vector3(innerRadius, 0f, -0.5f * outerRadius), // SE
            new Vector3(0f, 0f, -outerRadius), // S
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius), // SW
            new Vector3(-innerRadius, 0f,  0.5f * outerRadius) // NW
        };

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

        public static Vector3 GetBridge(HexDirection d)
        {
            return (GetFirstCorner(d) + GetSecondCorner(d)) * blendFactor;
        }
    }
}

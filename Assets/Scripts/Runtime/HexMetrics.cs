using UnityEngine;

namespace FourEx
{
    public static class HexMetrics
    {
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * 0.866025404f; // sqrt(3) / 2

        public const float solidFactor = 0.75f;
        public const float blendFactor = 1f - solidFactor;

        static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f,  0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f,  0.5f * outerRadius)
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

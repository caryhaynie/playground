using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public struct EdgeVertices
    {
        public Vector3 v1, v2, v3, v4, v5;

        public EdgeVertices(Vector3 corner1, Vector3 corner2)
        {
            v1 = corner1;
            v2 = Vector3.Lerp(corner1, corner2, 1f / 4f);
            v3 = Vector3.Lerp(corner1, corner2, 2f / 4f);
            v4 = Vector3.Lerp(corner1, corner2, 3f / 4f);
            v5 = corner2;
        }

        public EdgeVertices(Vector3 corner1, Vector3 corner2, float outerStep)
        {
            v1 = corner1;
            v2 = Vector3.Lerp(corner1, corner2, outerStep);
            v3 = Vector3.Lerp(corner1, corner2, 0.5f);
            v4 = Vector3.Lerp(corner1, corner2, 1f - outerStep);
            v5 = corner2;
        }

        public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
        {
            return new EdgeVertices {
                v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step),
                v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step),
                v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step),
                v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step),
                v5 = HexMetrics.TerraceLerp(a.v5, b.v5, step)
            };
        }
    }
}
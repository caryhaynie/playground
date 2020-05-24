using UnityEngine;

namespace PlayfulSoftware.Splines.Hybrid
{
    public static class Bezier
    {
        // B(t) = (1-t)^2 * p0 + 2 (1-t) * t * p1 + t^2 * p2
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            var oneMinusT = 1f - t;

            return oneMinusT.Squared() * p0 +
                   2f * oneMinusT * t * p1 +
                   t.Squared() * p2;
        }

        //  B(t) = (1 - t)^3 P0 + 3 (1 - t)^2 t P1 + 3 (1 - t) t^2 P2 + t^3 P3
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            var oneMinusT = 1f - t;

            return oneMinusT.Cubed() * p0 +
                   3f * oneMinusT.Squared() * t * p1 +
                   3f * oneMinusT * t.Squared() * p2 +
                   t.Cubed() * p3;
        }

        // B'(t) = 2 (1-t) (p1 - p0) + 2 * t * (p2 - p1)
        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
            => 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);

        // B'(t) = 3 (1 - t)^2 (P1 - P0) + 6 (1 - t) t (P2 - P1) + 3 t^2 (P3 - P2).
        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            var oneMinusT = 1f - t;

            return 3f * oneMinusT.Squared() * (p1 - p0) +
                   6f * oneMinusT * t * (p2 - p1) +
                   3f * t.Squared() * (p3 - p2);
        }

        private static float Cubed(this float v)
            => v * v * v;

        private static float Squared(this float v)
            => v * v;
    }
}
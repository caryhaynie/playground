using UnityEngine;

namespace PlayfulSoftwre.Splines.Hybrid
{
    public static class Bezier
    {
        // B(t) = (1-t)^2 * p0 + 2 (1-t) * t * p1 + t^2 * p2
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            var oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * p0 +
                   2f * oneMinusT * t * p1 +
                   t * t * p2;
        }

        // B'(t) = 2 (1-t) (p1 - p0) + 2 * t * (p2 - p1)
        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
            => 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }
}
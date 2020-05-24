using System;
using UnityEngine;

namespace PlayfulSoftware.Splines.Hybrid
{
    public sealed class SplineDecorator : MonoBehaviour
    {
        public BezierSpline spline;
        public int frequency;
        public bool lookForward;

        public Transform[] items;

        void Awake()
        {
            if (frequency <= 0 || items == null || items.Length == 0 || !spline)
                return;
            var stepSize = (float)frequency * items.Length;
            if (spline.loop || stepSize == 1f)
                stepSize = 1f / stepSize;
            else
                stepSize = 1f / (stepSize - 1f);

            for (int p = 0, f = 0; f < frequency; f++)
            for (int i = 0; i < items.Length; i++, p++)
            {
                var item = Instantiate(items[i], transform);
                var pos = spline.GetPoint(p * stepSize);
                item.localPosition = pos;
                if (lookForward)
                    item.LookAt(pos + spline.GetDirection(p * stepSize));
            }
        }
    }
}
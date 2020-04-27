using System;
using UnityEngine;

namespace PlayfulSoftware.Splines.Hybrid
{
    public sealed class SplineWalker : MonoBehaviour
    {
        public BezierSpline spline;
        public float duration;
        public bool lookForward;
        public SplineWalkerMode mode;

        private float m_Progress;
        private bool m_GoingForward;

        void Update()
        {
            if (!spline)
                return;

            UpdateProgress();

            var position = spline.GetPoint(m_Progress);
            transform.localPosition = position;
            if (lookForward)
                transform.LookAt(position + spline.GetDirection(m_Progress));
        }

        void UpdateProgress()
        {
            if (m_GoingForward)
            {
                m_Progress += Time.deltaTime / duration;
                if (m_Progress > 1f)
                {
                    switch (mode)
                    {
                        case SplineWalkerMode.Once:
                            m_Progress = 1f;
                            break;
                        case SplineWalkerMode.Loop:
                            m_Progress -= 1f;
                            break;
                        case SplineWalkerMode.PingPong:
                            m_Progress = 2f - m_Progress;
                            m_GoingForward = false;
                            break;
                        default:
                            throw new Exception($"Unhandled SplineWalkerMode: {mode}");
                    }
                }
            }
            else
            {
                m_Progress -= Time.deltaTime / duration;
                if (m_Progress < 0f)
                {
                    m_Progress = -m_Progress;
                    m_GoingForward = true;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayfulSoftwre.Splines.Hybrid
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(BezierCurve))]
    class BezierCurveInspector : Editor
    {
        BezierCurve m_Curve;
        Quaternion m_HandleRotation;
        Transform m_HandleTransform;

        const int lineSteps = 10;

        void OnSceneGUI()
        {
            m_Curve = target as BezierCurve;
            if (!m_Curve)
                return;
            m_HandleTransform = m_Curve.transform;
            m_HandleRotation = Tools.pivotRotation == PivotRotation.Local
                ? m_HandleTransform.rotation
                : Quaternion.identity;

            var p0 = ShowPoint(0);
            var p1 = ShowPoint(1);
            var p2 = ShowPoint(2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p1, p2);

            Handles.color = Color.white;
            var lineStart = m_Curve.GetPoint(0f);
            Handles.color = Color.green;
            Handles.DrawLine(lineStart, lineStart + m_Curve.GetDirection(0f));
            foreach (var t in GetStepPoints())
            {
                var lineEnd = m_Curve.GetPoint(t);
                Handles.color = Color.white;
                Handles.DrawLine(lineStart, lineEnd);
                Handles.color = Color.green;
                Handles.DrawLine(lineEnd, lineEnd + m_Curve.GetDirection(t));
                lineStart = lineEnd;
            }
        }

        IEnumerable<float> GetStepPoints()
        {
            for (var i = 0; i <= lineSteps; i++)
                yield return i / (float) lineSteps;
        }

        Vector3 ShowPoint(int index)
        {
            if (index < 0 || index > m_Curve.points.Length)
                throw new IndexOutOfRangeException();
            var point = m_HandleTransform.TransformPoint(m_Curve.points[index]);
            EditorGUI.BeginChangeCheck();
            {
                point = Handles.DoPositionHandle(point, m_HandleRotation);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Curve, "Move Point");
                EditorUtility.SetDirty(m_Curve);
                m_Curve.points[index] = m_HandleTransform.InverseTransformPoint(point);
            }

            return point;
        }
    }
#endif // UNITY_EDITOR
    public sealed class BezierCurve : MonoBehaviour
    {
        public Vector3[] points;

        void Reset()
        {
            points = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f)
            };
        }

        public Vector3 GetDirection(float t)
            => GetVelocity(t).normalized;

        public Vector3 GetPoint(float t)
            => transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], t));

        public Vector3 GetVelocity(float t)
            => transform.TransformPoint(Bezier.GetFirstDerivative(points[0], points[1], points[2], t))
               - transform.position;
    }
}
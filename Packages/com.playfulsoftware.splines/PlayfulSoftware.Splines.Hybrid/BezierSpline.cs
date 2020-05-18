using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PlayfulSoftware.Splines.Hybrid
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(BezierSpline))]
    class BezierSplineInspector : Editor
    {
        static Color[] modeColors =
        {
            Color.white,
            Color.yellow,
            Color.cyan
        };

        BezierSpline m_Spline;
        Quaternion m_HandleRotation;
        Transform m_HandleTransform;

        const float directionScale = 0.5f;
        const float handleSize = 0.04f;
        const float pickSize = 0.06f;
        const int stepsPerCurve = 10;

        int m_SelectedIndex = -1;

        void OnSceneGUI()
        {
            m_Spline = target as BezierSpline;
            if (!m_Spline)
                return;
            m_HandleTransform = m_Spline.transform;
            m_HandleRotation = Tools.pivotRotation == PivotRotation.Local
                ? m_HandleTransform.rotation
                : Quaternion.identity;

            var p0 = ShowPoint(0);

            for (var i = 1; i < m_Spline.controlPointCount; i += 3)
            {
                var p1 = ShowPoint(i);
                var p2 = ShowPoint(i + 1);
                var p3 = ShowPoint(i + 2);

                Handles.color = Color.gray;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);

                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
                p0 = p3;
            }

            ShowDirections();
        }

        public override void OnInspectorGUI()
        {
            m_Spline = target as BezierSpline;
            if (!m_Spline)
                return;

            EditorGUI.BeginChangeCheck();
            var loop = EditorGUILayout.Toggle("Loop", m_Spline.loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Spline, "Toggle Loop");
                m_Spline.loop = loop;
                EditorUtility.SetDirty(m_Spline);
            }
            if (m_SelectedIndex >= 0 && m_SelectedIndex < m_Spline.controlPointCount)
            {
                DrawSelectedPointInspector();
            }
            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(m_Spline, "Add Curve");
                m_Spline.AddCurve();
                EditorUtility.SetDirty(m_Spline);
            }
        }

        void DrawSelectedPointInspector()
        {
            GUILayout.Label("Selected Point");
            EditorGUI.BeginChangeCheck();
            var point = EditorGUILayout.Vector3Field(
                "Position",
                m_Spline.GetControlPoint(m_SelectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Spline, "Move Point");
                EditorUtility.SetDirty(m_Spline);
                m_Spline.SetControlPoint(m_SelectedIndex, point);
            }
            EditorGUI.BeginChangeCheck();
            var mode = (BezierControlPointMode)
                EditorGUILayout.EnumPopup("Mode", m_Spline.GetControlPointMode(m_SelectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Spline, "Change Point Mode");
                m_Spline.SetControlPointMode(m_SelectedIndex, mode);
                EditorUtility.SetDirty(m_Spline);
            }
        }

        IEnumerable<float> GetStepPoints()
        {
            var steps = stepsPerCurve * m_Spline.curveCount;
            for (var i = 0; i <= steps; i++)
                yield return i / (float) steps;
        }

        void ShowDirections()
        {
            var point = m_Spline.GetPoint(0f);
            Handles.color = Color.green;
            Handles.DrawLine(point, point + m_Spline.GetDirection(0f) * directionScale);
            foreach (var t in GetStepPoints())
            {
                point = m_Spline.GetPoint(t);
                Handles.DrawLine(point, point + m_Spline.GetDirection(t) * directionScale);
            }
        }

        Vector3 ShowPoint(int index)
        {
            if (index < 0 || index > m_Spline.controlPointCount)
                throw new IndexOutOfRangeException();
            var point = m_HandleTransform.TransformPoint(m_Spline.GetControlPoint(index));
            var size = HandleUtility.GetHandleSize(point);
            // make first point larger for easy identification
            if (index == 0)
                size *= 2f;
            Handles.color = modeColors[(int) m_Spline.GetControlPointMode(index)];
            if (Handles.Button(point, m_HandleRotation, handleSize * size, pickSize * size, Handles.DotHandleCap))
            {
                m_SelectedIndex = index;
                Repaint();
            }

            if (m_SelectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();
                {
                    point = Handles.DoPositionHandle(point, m_HandleRotation);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_Spline, "Move Point");
                    EditorUtility.SetDirty(m_Spline);
                    m_Spline.SetControlPoint(index, m_HandleTransform.InverseTransformPoint(point));
                }
            }

            return point;
        }
    }
#endif // UNITY_EDITOR

    public sealed class BezierSpline : MonoBehaviour
    {
        [SerializeField] private BezierControlPointMode[] m_Modes;
        [SerializeField] private Vector3[] m_Points;
        [SerializeField] private bool m_Loop;

        public int controlPointCount => m_Points.Length;
        public int curveCount => (m_Points.Length - 1) / 3;

        public bool loop
        {
            get => m_Loop;
            set
            {
                m_Loop = value;
                if (value)
                {
                    m_Modes[m_Modes.Length - 1] = m_Modes[0];
                    SetControlPoint(0, m_Points[0]);
                }
            }
        }

        void Reset()
        {
            m_Modes = new BezierControlPointMode[]
            {
                BezierControlPointMode.Free,
                BezierControlPointMode.Free
            };

            m_Points = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4f, 0f, 0f),
            };
        }

        public void AddCurve()
        {
            var point = m_Points[m_Points.Length - 1];
            Array.Resize(ref m_Points, m_Points.Length + 3);
            point.x += 1f;
            m_Points[m_Points.Length - 3] = point;
            point.x += 1f;
            m_Points[m_Points.Length - 2] = point;
            point.x += 1f;
            m_Points[m_Points.Length - 1] = point;

            Array.Resize(ref m_Modes, m_Modes.Length + 1);
            m_Modes[m_Modes.Length - 1] = m_Modes[m_Modes.Length - 2];
            EnforceMode(m_Points.Length - 4);

            if (m_Loop)
            {
                m_Points[m_Points.Length - 1] = m_Points[0];
                m_Modes[m_Modes.Length - 1] = m_Modes[0];
                EnforceMode(0);
            }
        }

        public Vector3 GetControlPoint(int index)
        {
            ValidatePointArrayAndThrow();
            return m_Points[index];
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            ValidatePointArrayAndThrow();
            if (index % 3 == 0)
            {
                var delta = point - m_Points[index];
                if (m_Loop)
                {
                    if (index == 0)
                    {
                        m_Points[1] += delta;
                        m_Points[m_Points.Length - 2] += delta;
                        m_Points[m_Points.Length - 1] = point;
                    }
                    else if (index == m_Points.Length - 1)
                    {
                        m_Points[0] = point;
                        m_Points[1] += delta;
                        m_Points[index - 1] += delta;
                    }
                    else
                    {
                        m_Points[index - 1] += delta;
                        m_Points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                        m_Points[index - 1] += delta;
                    if (index + 1 < m_Points.Length)
                        m_Points[index + 1] += delta;
                }
            }
            m_Points[index] = point;
            EnforceMode(index);
        }

        public BezierControlPointMode GetControlPointMode(int index)
        {
            ValidateModeArrayAndThrow();
            return m_Modes[GetModeIndex(index)];
        }

        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            ValidateModeArrayAndThrow();
            var modeIndex = GetModeIndex(index);
            m_Modes[modeIndex] = mode;
            if (m_Loop)
            {
                if (modeIndex == 0)
                    m_Modes[m_Modes.Length - 1] = mode;
                else if (modeIndex == m_Modes.Length - 1)
                    m_Modes[0] = mode;
            }
            EnforceMode(index);
        }


        void EnforceMode(int index)
        {
            var modeIndex = GetModeIndex(index);
            var mode = m_Modes[modeIndex];
            var isFreeMode = mode == BezierControlPointMode.Free;
            var isAtEnd = !m_Loop && (modeIndex == 0 || modeIndex == m_Modes.Length - 1);
            if (isFreeMode || isAtEnd)
                return;
            var middleIndex = modeIndex * 3;
            int fixedIndex = 0, enforcedIndex = 0;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                    fixedIndex = m_Points.Length - 2;
                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= m_Points.Length)
                    enforcedIndex = 1;
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= m_Points.Length)
                    fixedIndex = 1;
                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                    enforcedIndex = m_Points.Length - 2;
            }

            var middle = m_Points[middleIndex];
            var enforcedTangent = middle - m_Points[fixedIndex];
            if (mode == BezierControlPointMode.Aligned)
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, m_Points[enforcedIndex]);
            m_Points[enforcedIndex] = middle + enforcedTangent;
        }

        int GetModeIndex(int controlIndex) => (controlIndex + 1) / 3;

        int GetCurveIndex(ref float t)
        {
            if (t >= 1f)
            {
                t = 1f;
                return m_Points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * curveCount;
                var i = (int) t;
                t -= i;
                return i * 3;
            }
        }

        public Vector3 GetDirection(float t)
            => GetVelocity(t).normalized;

        public Vector3 GetPoint(float t)
        {
            ValidatePointArrayAndThrow();
            var i = GetCurveIndex(ref t);
            return transform.TransformPoint(
                Bezier.GetPoint(
                    m_Points[i],
                    m_Points[i + 1],
                    m_Points[i + 2],
                    m_Points[i + 3], t));
        }

        public Vector3 GetVelocity(float t)
        {
            ValidatePointArrayAndThrow();
            var i = GetCurveIndex(ref t);
            return transform.TransformPoint(
                       Bezier.GetFirstDerivative(
                           m_Points[i],
                           m_Points[i + 1],
                           m_Points[i + 2],
                           m_Points[i +3],t))
                   - transform.position;
        }

        [Conditional("DEBUG")]
        private void ValidateModeArrayAndThrow()
        {
            if (m_Modes == null)
                throw new Exception("Modes array is null");
        }

        [Conditional("DEBUG")]
        private void ValidatePointArrayAndThrow()
        {
            if (m_Points == null)
                throw new Exception("Points array is null");
            if (m_Points.Length % 3 != 1)
                throw new Exception("Invalid size of points array.");
        }
    }
}
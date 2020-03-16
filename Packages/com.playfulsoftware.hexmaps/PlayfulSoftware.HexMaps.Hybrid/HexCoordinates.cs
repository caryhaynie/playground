using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace PlayfulSoftware.HexMaps.Hybrid
{
    [Serializable]
    public struct HexCoordinates
    {
        [SerializeField]
        private int m_X;
        [SerializeField]
        private int m_Z;

        public int x
        {
            get { return m_X; }
            private set { m_X = value; }
        }
        public int y { get { return -x - z; } }
        public int z
        {
            get { return m_Z; }
            private set { m_Z = value; }
        }

        public HexCoordinates(int _x, int _z)
        {
            m_X = _x;
            m_Z = _z;
        }

        public static HexCoordinates FromOffsetCoordinates(int _x, int _z)
        {
            return new HexCoordinates(_x - _z / 2, _z);
        }

        public static HexCoordinates FromPosition(Vector3 position)
        {
            float x = position.x / (HexMetrics.innerRadius * 2f);
            float y = -x;
            var offset = position.z / (HexMetrics.outerRadius * 3f);
            x -= offset;
            y -= offset;

            var iX = Mathf.RoundToInt(x);
            var iY = Mathf.RoundToInt(y);
            var iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }
            return new HexCoordinates(iX, iZ);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        public string ToStringOnSeparateLines()
        {
            return string.Format("{0}\n{1}\n{2}", x, y, z);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HexCoordinates))]
    public class HexCoordinatesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int x = property.FindPropertyRelative("m_X").intValue;
            int z = property.FindPropertyRelative("m_Z").intValue;
            int y = -x - z;
            position = EditorGUI.PrefixLabel(position, label);
            GUI.Label(position, string.Format("{0}, {1}, {2}", x, y, z));
        }
    }
#endif // UNITY_EDITOR
}
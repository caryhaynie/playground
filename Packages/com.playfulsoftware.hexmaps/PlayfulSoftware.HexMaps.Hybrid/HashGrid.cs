using System;
using UnityEngine;
using UVector3 = UnityEngine.Vector3;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    [Serializable]
    internal sealed class HashGrid<T>
    {
        const int defaultGridAxisSize = 256;

        [SerializeField]
        readonly int m_AxisLength;
        [SerializeField]
        readonly T[] m_Grid;

        public float Scale { get; set; } = 0.25f;

        public HashGrid(Func<int, T> ctor, int axisLength = defaultGridAxisSize)
        {
            m_AxisLength = axisLength;
            m_Grid = new T[m_AxisLength * m_AxisLength];
            for (int i = 0; i < m_Grid.Length; i++)
            {
                m_Grid[i] = ctor(i);
            }
        }

        public T Sample(UVector3 position)
        {
            var x = (int) (position.x * Scale) % m_AxisLength;
            if (x < 0)
                x += m_AxisLength;
            var z = (int) (position.z * Scale) % m_AxisLength;
            if (z < 0)
                z += m_AxisLength;
            return m_Grid[x + z * m_AxisLength];
        }
    }
}
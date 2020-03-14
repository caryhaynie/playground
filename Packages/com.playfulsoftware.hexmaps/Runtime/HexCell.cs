using System;
using UnityEngine;

namespace PlayfulSoftware.HexMaps
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;
        private Color m_Color;

        [SerializeField] HexCell[] m_Neighbors = new HexCell[6];
        [SerializeField] int m_Elevation = Int32.MinValue;

        [HideInInspector] public HexGridChunk chunk;
        [HideInInspector] public RectTransform uiRect;

        public int elevation
        {
            get => m_Elevation;
            set
            {
                if (m_Elevation == value)
                    return;
                m_Elevation = value;

                // Update Transform
                var pos = transform.localPosition;
                pos.y = m_Elevation * HexMetrics.elevationStep;
                pos.y +=
                    (HexMetrics.SampleNoise(pos).y * 2f - 1f) *
                    HexMetrics.elevationPerturbStrength;
                transform.localPosition = pos;

                // Update UI Transform
                var uiPosition = uiRect.localPosition;
                uiPosition.z = -pos.y;
                uiRect.localPosition = uiPosition;
                Refresh();
            }
        }

        public Vector3 position => transform.localPosition;

        public Color color
        {
            get => m_Color;
            set
            {
                if (m_Color == value)
                    return;
                m_Color = value;

                Refresh();
            }
        }

        public HexEdgeType GetEdgeType(HexCell otherCell)
        {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return GetEdgeType(GetNeighbor(direction));
        }

        public HexCell GetNeighbor(HexDirection direction)
        {
            return m_Neighbors[(int) direction];
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            m_Neighbors[(int) direction] = cell;
            cell.m_Neighbors[(int) direction.Opposite()] = this;
        }

        void Refresh()
        {
            if (!chunk) return;
            chunk.Refresh();
            RefreshNeighborsIfInDifferentChunks();
        }

        void RefreshNeighborsIfInDifferentChunks()
        {
            for (int i = 0; i < m_Neighbors.Length; i++)
            {
                var nChunk = m_Neighbors[i]?.chunk;
                if (nChunk && nChunk != chunk)
                    nChunk.Refresh();
            }
        }
    
}
}
using UnityEngine;

namespace FourEx
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;
        public Color color;

        [SerializeField]
        HexCell[] m_Neighbors = new HexCell[6];
        [SerializeField]
        int m_Elevation;
        [HideInInspector]
        public RectTransform uiRect;
        public int elevation
        {
            get
            {
                return m_Elevation;
            }
            set
            {
                m_Elevation = value;
                UpdateTransform();
                UpdateUITransform();
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
            return m_Neighbors[(int)direction];
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            m_Neighbors[(int)direction] = cell;
            cell.m_Neighbors[(int)direction.Opposite()] = this;
        }

        private void UpdateTransform()
        {
            var position = transform.localPosition;
            position.y = m_Elevation * HexMetrics.elevationStep;
            transform.localPosition = position;
        }

        private void UpdateUITransform()
        {
            var position = uiRect.localPosition;
            position.z = m_Elevation * -HexMetrics.elevationStep;
            uiRect.localPosition = position;
        }
    }
}
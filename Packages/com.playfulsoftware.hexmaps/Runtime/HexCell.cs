using UnityEngine;

namespace PlayfulSoftware.HexMaps
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

                // Update Transform
                var position = transform.localPosition;
                position.y = m_Elevation * HexMetrics.elevationStep;
                position.y +=
                    (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                    HexMetrics.elevationPerturbStrength;
                transform.localPosition = position;

                // Update UI Transform
                var uiPosition = uiRect.localPosition;
                uiPosition.z = -position.y;
                uiRect.localPosition = uiPosition;
            }
        }

        public Vector3 position
        {
            get { return transform.localPosition; }
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
    }
}
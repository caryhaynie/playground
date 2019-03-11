using UnityEngine;

namespace FourEx
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;

        public Color color;

        [SerializeField]
        HexCell[] m_Neighbors = new HexCell[6];

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
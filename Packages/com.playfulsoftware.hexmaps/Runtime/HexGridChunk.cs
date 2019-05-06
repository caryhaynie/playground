using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSoftware.HexMaps
{
    public class HexGridChunk : MonoBehaviour
    {
        private HexCell[] m_Cells;
        private Canvas m_GridCanvas;
        private HexMesh m_HexMesh;

        void Awake()
        {
            m_GridCanvas = GetComponentInChildren<Canvas>();
            m_HexMesh = GetComponentInChildren<HexMesh>();

            m_Cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        }

        void Start()
        {
            m_HexMesh.Triangulate(m_Cells);
        }

        public void AddCell(int index, HexCell cell)
        {
            m_Cells[index] = cell;
            cell.transform.SetParent(transform, false);
            cell.uiRect.SetParent(m_GridCanvas.transform, false);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexGridChunk : MonoBehaviour
    {
        private HexCell[] m_Cells;
        private Canvas m_GridCanvas;
        private HexMesh m_HexMesh;

        void Awake()
        {
            m_GridCanvas = GetComponentInChildren<Canvas>();
            m_HexMesh = GetComponentInChildren<HexMesh>();

            m_Cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
            ShowUI(false);
        }

        public void AddCell(int index, HexCell cell)
        {
            m_Cells[index] = cell;
            cell.chunk = this;
            cell.transform.SetParent(transform, false);
            cell.uiRect.SetParent(m_GridCanvas.transform, false);
        }

        public void Refresh()
        {
            enabled = true;
        }

        public void ShowUI(bool visible)
        {
            m_GridCanvas.gameObject.SetActive(visible);
        }

        void LateUpdate()
        {
            m_HexMesh.Triangulate(m_Cells);
            enabled = false;
        }
    }
}
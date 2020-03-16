using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexMapEditor : MonoBehaviour
    {
        private Color m_ActiveColor;
        private int m_ActiveElevation;
        private bool m_ApplyColor;
        private bool m_ApplyElevation;
        private int m_BrushSize;
        [SerializeField]
        private HexGrid m_HexGrid;
        [SerializeField]
        private Color[] m_Colors;
        public Color[] colors
        {
            get { return m_Colors; }
            set { m_Colors = value; }
        }
        public HexGrid hexGrid
        {
            get { return m_HexGrid; }
            set { m_HexGrid = value; }
        }

        void Awake()
        {
            SelectColor(0);
        }

        void HandleInput()
        {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
                EditCells(m_HexGrid.GetCell(hit.point));
        }

        void EditCells(HexCell center)
        {
            int centerX = center.coordinates.x;
            int centerZ = center.coordinates.z;

            for (int r = 0, z = centerZ - m_BrushSize; z <= centerZ; z++, r++)
            for (int x = centerX - r; x <= centerX + m_BrushSize; x++)
            {
                EditCell(m_HexGrid.GetCell(new HexCoordinates(x, z)));
            }

            for (int r = 0, z = centerZ + m_BrushSize; z > centerZ; z--, r++)
            for (int x = centerX - m_BrushSize; x <= centerX + r; x++)
            {
                EditCell(m_HexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        void EditCell(HexCell cell)
        {
            if (!cell)
                return;
            if (m_ApplyColor)
                cell.color = m_ActiveColor;
            if (m_ApplyElevation)
                cell.elevation = m_ActiveElevation;
        }

        public void SelectColor(int index)
        {
            m_ApplyColor = index >= 0;
            if (m_ApplyColor)
                m_ActiveColor = m_Colors[index];
        }

        public void SetApplyElevation(bool toggle)
        {
            m_ApplyElevation = toggle;
        }

        public void SetBrushSize(float brushSize)
        {
            m_BrushSize = (int)brushSize;
        }

        public void SetElevation(float elevation)
        {
            m_ActiveElevation = (int)elevation;
        }

        public void ShowUI(bool visible)
        {
            m_HexGrid.ShowUI(visible);
        }

        void Update()
        {
            var isClick = Input.GetMouseButton(0);
            var isOverGO = EventSystem.current.IsPointerOverGameObject();
            if (isClick && !isOverGO)
                HandleInput();
        }
    }
}
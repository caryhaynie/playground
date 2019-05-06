using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayfulSoftware.HexMaps
{
    public class HexMapEditor : MonoBehaviour
    {
        private Color m_ActiveColor;
        private int m_ActiveElevation;
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
                EditCell(m_HexGrid.GetCell(hit.point));
        }

        void EditCell(HexCell cell)
        {
            cell.color = m_ActiveColor;
            cell.elevation = m_ActiveElevation;
        }

        public void SelectColor(int index)
        {
            m_ActiveColor = m_Colors[index];
        }

        public void SetElevation(float elevation)
        {
            m_ActiveElevation = (int)elevation;
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
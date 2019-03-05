using UnityEngine;
using UnityEngine.EventSystems;

namespace FourEx
{
    public class HexMapEditor : MonoBehaviour
    {
        private Color m_ActiveColor;
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
                m_HexGrid.TouchCell(hit.point, m_ActiveColor);
        }

        public void SelectColor(int index)
        {
            m_ActiveColor = m_Colors[index];
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
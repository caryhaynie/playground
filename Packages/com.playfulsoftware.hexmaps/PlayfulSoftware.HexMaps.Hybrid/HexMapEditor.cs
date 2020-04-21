using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexMapEditor : MonoBehaviour
    {
        enum OptionalToggle
        {
            Ignore, Yes, No
        }

        struct EditMode
        {
            public bool enabled;
            public int level;
        }

        #region Non-Serialized Fields
        Color m_ActiveColor;
        bool m_ApplyColor;
        EditMode m_Elevation;
        EditMode m_FarmLevel;
        EditMode m_PlantLevel;
        EditMode m_UrbanLevel;
        EditMode m_WaterLevel;
        int m_BrushSize;
        OptionalToggle m_RiverMode = OptionalToggle.Ignore;
        OptionalToggle m_RoadMode = OptionalToggle.Ignore;
        #endregion

        #region Drag-related Fields
        HexDirection m_DragDirection;
        bool m_IsDrag;
        HexCell m_PreviousCell;
        #endregion

        #region Serialized Fields
        [SerializeField]
        HexGrid m_HexGrid;
        [SerializeField]
        Color[] m_Colors;
        #endregion

        public Color[] colors
        {
            get => m_Colors;
            set => m_Colors = value;
        }

        public HexGrid hexGrid
        {
            get => m_HexGrid;
            set => m_HexGrid = value;
        }

        void Awake()
        {
            SelectColor(-1);
        }

        void HandleInput()
        {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                var currentCell = m_HexGrid.GetCell(hit.point);
                if (m_PreviousCell && m_PreviousCell != currentCell)
                {
                    ValidateDrag(currentCell);
                }
                else
                {
                    m_IsDrag = false;
                }
                EditCells(currentCell);
                m_PreviousCell = currentCell;
            }
            else
            {
                m_PreviousCell = null;
            }
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
            if (m_Elevation.enabled)
                cell.elevation = m_Elevation.level;
            if (m_FarmLevel.enabled)
                cell.farmLevel = m_FarmLevel.level;
            if (m_PlantLevel.enabled)
                cell.plantLevel = m_PlantLevel.level;
            if (m_UrbanLevel.enabled)
                cell.urbanLevel = m_UrbanLevel.level;
            if (m_WaterLevel.enabled)
                cell.waterLevel = m_WaterLevel.level;
            if (m_RiverMode == OptionalToggle.No)
                cell.RemoveRiver();
            if (m_RoadMode == OptionalToggle.No)
                cell.RemoveRoads();
            if (m_IsDrag)
            {
                var otherCell = cell.GetNeighbor(m_DragDirection.Opposite());
                if (otherCell)
                {
                    if (m_RiverMode == OptionalToggle.Yes)
                        otherCell.SetOutgoingRiver(m_DragDirection);
                    if (m_RoadMode == OptionalToggle.Yes)
                        otherCell.AddRoad(m_DragDirection);
                }
            }
        }

        public void SelectColor(int index)
        {
            m_ApplyColor = index >= 0;
            if (m_ApplyColor)
                m_ActiveColor = m_Colors[index];
        }

        public void SetApplyElevation(bool toggle)
        {
            m_Elevation.enabled = toggle;
        }

        public void SetApplyFarmLevel(bool toggle)
        {
            m_FarmLevel.enabled = toggle;
        }

        public void SetApplyPlantLevel(bool toggle)
        {
            m_PlantLevel.enabled = toggle;
        }

        public void SetApplyUrbanLevel(bool toggle)
        {
            m_UrbanLevel.enabled = toggle;
        }

        public void SetApplyWaterLevel(bool toggle)
        {
            m_WaterLevel.enabled = toggle;
        }

        public void SetBrushSize(float brushSize)
        {
            m_BrushSize = (int)brushSize;
        }

        public void SetElevation(float elevation)
        {
            m_Elevation.level = (int)elevation;
        }

        public void SetRiverMode(int mode)
        {
            m_RiverMode = (OptionalToggle) mode;
        }

        public void SetRoadMode(int mode)
        {
            m_RoadMode = (OptionalToggle) mode;
        }

        public void SetFarmLevel(float level)
        {
            m_FarmLevel.level = (int) level;
        }

        public void SetPlantLevel(float level)
        {
            m_PlantLevel.level = (int) level;
        }

        public void SetUrbanLevel(float level)
        {
            m_UrbanLevel.level = (int) level;
        }

        public void SetWaterLevel(float level)
        {
            m_WaterLevel.level = (int)level;
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
            else
            {
                m_PreviousCell = null;
            }
        }

        void ValidateDrag(HexCell currentCell)
        {
            for (m_DragDirection = HexDirection.NE; m_DragDirection <= HexDirection.NW; m_DragDirection++)
            {
                if (m_PreviousCell.GetNeighbor(m_DragDirection) == currentCell)
                {
                    m_IsDrag = true;
                    return;
                }
            }

            m_IsDrag = false;
        }
    }
}
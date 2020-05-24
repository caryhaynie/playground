using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.WSA;

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
        int m_ActiveTerrainTypeIndex;
        EditMode m_Elevation;
        EditMode m_FarmLevel;
        EditMode m_PlantLevel;
        EditMode m_SpecialLevel;
        EditMode m_UrbanLevel;
        EditMode m_WaterLevel;
        int m_BrushSize;
        OptionalToggle m_RiverMode = OptionalToggle.Ignore;
        OptionalToggle m_RoadMode = OptionalToggle.Ignore;
        OptionalToggle m_WalledMode = OptionalToggle.Ignore;
        #endregion

        #region Drag-related Fields
        HexDirection m_DragDirection;
        bool m_IsDrag;
        HexCell m_PreviousCell;
        #endregion

        #region Serialized Fields
        [SerializeField]
        HexGrid m_HexGrid;
        #endregion

        public HexGrid hexGrid
        {
            get => m_HexGrid;
            set => m_HexGrid = value;
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
            if (m_SpecialLevel.enabled)
                cell.specialIndex = m_SpecialLevel.level;
            if (m_RiverMode == OptionalToggle.No)
                cell.RemoveRiver();
            if (m_RoadMode == OptionalToggle.No)
                cell.RemoveRoads();
            if (m_WalledMode != OptionalToggle.Ignore)
                cell.walled = m_WalledMode == OptionalToggle.Yes;
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

        public void SetApplySpecialLevel(bool toggle)
        {
            m_SpecialLevel.enabled = toggle;
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

        public void SetSpecialLevel(float level)
        {
            m_SpecialLevel.level = (int) level;
        }

        public void SetTerrainTypeIndex(int index)
            => m_ActiveTerrainTypeIndex = index;


        public void SetUrbanLevel(float level)
        {
            m_UrbanLevel.level = (int) level;
        }

        public void SetWalledMode(int mode)
        {
            m_WalledMode = (OptionalToggle) mode;
        }

        public void SetWaterLevel(float level)
        {
            m_WaterLevel.level = (int)level;
        }

        public void ShowUI(bool visible)
        {
            m_HexGrid.ShowUI(visible);
        }

        public void Load()
        {
            var path = Path.Combine(DefaultMapPath(), "test.map");
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                int header = reader.ReadInt32();
                if (header == 0)
                    hexGrid.Load(reader);
                else
                    Debug.LogWarning($"Unknown map format {header}");
            }
        }

        public void Save()
        {
            //DebugHelper.LogNoStacktrace(DefaultMapPath());
            var path = Path.Combine(DefaultMapPath(), "test.map");
            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(0);
                hexGrid.Save(writer);
            }
        }

        string DefaultMapPath()
        {
#if UNITY_EDITOR
            const string kPath = "Assets/GameData/Maps";
            if (!Directory.Exists(kPath))
                Directory.CreateDirectory(kPath);
            return kPath;
#else
            return Application.persistentDataPath;
#endif // UNITY_EDITOR
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
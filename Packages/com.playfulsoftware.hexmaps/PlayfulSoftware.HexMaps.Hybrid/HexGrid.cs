using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSoftware.HexMaps.Hybrid
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(HexGrid))]
    internal sealed class HexGridEditor : Editor
    {
        const string kShouldShowInspectorKey = "HexGridEditor_ShouldShowInspector";

        private SerializedProperty m_CellsProp;
        private SerializedProperty m_ChunksProp;

        internal bool ShouldShowInspector
        {
            get => SessionState.GetBool(kShouldShowInspectorKey, false);
            set => SessionState.SetBool(kShouldShowInspectorKey, value);
        }

        private HexGrid typedTarget => (HexGrid)target;

        void OnEnable()
        {
            m_CellsProp = serializedObject.FindProperty("m_Cells");
            m_ChunksProp = serializedObject.FindProperty("m_Chunks");
        }

        public override void OnInspectorGUI()
        {
            var grid = typedTarget;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                bool showInspector = EditorGUILayout.Foldout(ShouldShowInspector, "Inspector Values");
                if (check.changed)
                    ShouldShowInspector = showInspector;
            }

            if (ShouldShowInspector)
                DrawDefaultInspector();
            if (GUILayout.Button("Rebuild Hex Map"))
            {
                Undo.RecordObject(grid, "Rebuild Hex Map");
                grid.RebuildGrid(forceRebuild: true);
                EditorUtility.SetDirty(grid);
            }

            var childChunks = m_ChunksProp.arraySize;
            EditorGUILayout.LabelField($"Existing Chunks: {childChunks}");
            using (new EditorGUI.DisabledScope(childChunks == 0))
            {
                if (GUILayout.Button("Delete Existing Chunks"))
                {
                    Undo.RecordObject(grid, "Delete Existing Chunks");
                    grid.RemoveExistingChunks();
                }
            }

        }

        private bool HasCreatedChunks()
        {
            var obj = typedTarget;
            if (!obj) return false;
            if (m_ChunksProp == null) return false;
            if (!m_ChunksProp.isArray) return false;
            return m_ChunksProp.arraySize != 0;
        }
    }
#endif // UNITY_EDITOR
    [ExecuteAlways]
    public sealed class HexGrid : MonoBehaviour
    {
        [HideInInspector,SerializeField]
        private int m_CellCountX;
        [HideInInspector,SerializeField]
        private int m_CellCountZ;
        [SerializeField]
        private int m_ChunkCountX;
        [SerializeField]
        private int m_ChunkCountZ;
        [SerializeField]
        private Color m_DefaultColor = Color.white;
        [SerializeField]
        private HexCell m_CellPrefab;
        [SerializeField]
        private Text m_CellLabelPrefab;
        [SerializeField]
        private HexGridChunk m_ChunkPrefab;
#pragma warning disable 0649
        [SerializeField]
        private HexMapGenerationParameters m_ParametersAsset;
#pragma warning restore 0649

        public int seed;

        [HideInInspector,SerializeField]
        private HexCell[] m_Cells;
        [HideInInspector,SerializeField]
        private HexGridChunk[] m_Chunks;

        private int cellCountX => m_CellCountX;
        private int cellCountZ => m_CellCountZ;
        public int chunkCountX
        {
            get => m_ChunkCountX;
            set => m_CellCountX = value;
        }
        public int chunkCountZ
        {
            get => m_ChunkCountZ;
            set => m_ChunkCountZ = value;
        }
        public Color defaultColor
        {
            get => m_DefaultColor;
            set => m_DefaultColor = value;
        }
        public HexCell cellPrefab => m_CellPrefab;
        public Text cellLabelPrefab => m_CellLabelPrefab;
        public HexGridChunk chunkPrefab => m_ChunkPrefab;

        bool IsInPlayMode => Application.IsPlaying(gameObject);

        void Awake()
        {
            if (IsInPlayMode)
                AwakePlaymode();
            else
                AwakeEditmode();
        }

        void AwakeEditmode()
        {
        }

        void AwakePlaymode()
        {
            SetParameters(forceOverride: true);

            RebuildGrid();
        }

        void AddCellToChunk(int x, int z, HexCell cell)
        {
            int chunkX = x / HexMetrics.chunkSizeX;
            int chunkZ = z / HexMetrics.chunkSizeZ;
            HexGridChunk chunk = m_Chunks[chunkX + chunkZ * chunkCountX];

            int localX = x - chunkX * HexMetrics.chunkSizeX;
            int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
            chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
        }

        void CreateCells()
        {
            m_Cells = new HexCell[cellCountZ * cellCountX];
            for (int z = 0, i = 0; z < cellCountZ; z++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        void CreateCell(int x, int z, int i)
        {
            Vector3 position = new Vector3 {
                x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f),
                y = 0f,
                z = z * (HexMetrics.outerRadius * 1.5f)
            };

            HexCell cell = m_Cells[i] = Instantiate<HexCell>(m_CellPrefab);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.name = $"Cell #{i} {cell.coordinates}";
            cell.color = defaultColor;


            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, m_Cells[i - 1]);
            }
            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, m_Cells[i - cellCountX]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, m_Cells[i - cellCountX - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, m_Cells[i - cellCountX]);
                    if (x < cellCountX - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, m_Cells[i - cellCountX + 1]);
                    }
                }
            }

            Text label = Instantiate<Text>(m_CellLabelPrefab);
            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();

            cell.uiRect = label.rectTransform;
            cell.elevation = 0;

            AddCellToChunk(x, z, cell);
        }

        void CreateChunks()
        {
            m_Chunks = new HexGridChunk[chunkCountX * chunkCountZ];
            for (int z = 0, i = 0; z < chunkCountZ; z++)
            {
                for (int x = 0; x < chunkCountX; x++)
                {
                    HexGridChunk chunk = m_Chunks[i++] = Instantiate(m_ChunkPrefab);
                    chunk.name = $"Chunk #{i}";
                    chunk.transform.SetParent(transform);
                }
            }
        }

        void OnEnable()
        {
            SetParameters();
        }

        internal void RebuildGrid(bool forceRebuild = false)
        {
            var gridExists = m_Chunks != null && m_Chunks.Length > 0;
            if (gridExists && !forceRebuild)
                return;
            RemoveExistingChunks();
            m_CellCountX = m_ChunkCountX * HexMetrics.chunkSizeX;
            m_CellCountZ = m_ChunkCountZ * HexMetrics.chunkSizeZ;

            CreateChunks();
            CreateCells();
        }

        internal void RemoveExistingChunks()
        {
            foreach (var chunk in m_Chunks)
            {
                if (!chunk) continue;
                GameObjectUtility.SafelyDeleteGameObject(chunk.gameObject);
            }
            m_Chunks = new HexGridChunk[0];
        }

        void SetParameters(bool forceOverride = false)
        {
            if (!m_ParametersAsset)
                return;
            if (forceOverride || !HexMetrics.parametersAsset)
                HexMetrics.parametersAsset = m_ParametersAsset;
        }

#if UNITY_EDITOR
        void OnValidate()
        {

        }

        string PackagePrefab(string prefab) =>
            $"Packages/com.playfulsoftware.hexmaps/Prefabs/{prefab}";

        internal void RemoveExistingCells()
        {
            if (IsInPlayMode)
                throw new Exception("This method should not be called from play mode");
            foreach (var cell in m_Cells)
            {
                if (!cell) continue;
                DestroyImmediate(cell.gameObject);
            }
            m_Cells = new HexCell[0];
        }

        void Reset()
        {
            m_ChunkCountX = 6;
            m_ChunkCountZ = 6;
            if (!m_CellLabelPrefab)
                m_CellLabelPrefab =
                    AssetDatabase.LoadAssetAtPath<Text>(PackagePrefab("CellLabel.Prefab"));
            if (!m_CellPrefab)
                m_CellPrefab =
                    AssetDatabase.LoadAssetAtPath<HexCell>(PackagePrefab("HexCellPrefab.prefab"));
            if (!m_ChunkPrefab)
                m_ChunkPrefab =
                    AssetDatabase.LoadAssetAtPath<HexGridChunk>(PackagePrefab("GridChunkPrefab.prefab"));
        }

        void Update()
        {

        }

#endif // UNITY_EDITOR

        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            var coordinates = HexCoordinates.FromPosition(position);
            var index = coordinates.x + coordinates.z * cellCountX + coordinates.z / 2;
            //Debug.LogFormat("touched at {0} (index: {1})", coordinates, index);
            return m_Cells[index];
        }

        public HexCell GetCell(HexCoordinates coordinates)
        {
            int z = coordinates.z;
            if (z < 0 || z >= cellCountZ)
                return null;
            int x = coordinates.x + z / 2;
            if (x < 0 || x >= cellCountX)
                return null;
            return m_Cells[x + z * cellCountX];
        }

        public void ShowUI(bool visible)
        {
            foreach (var chunk in m_Chunks)
                chunk.ShowUI(visible);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSoftware.HexMaps
{
    public class HexGrid : MonoBehaviour
    {
        private int m_CellCountX;
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
        [SerializeField]
        private Texture2D m_NoiseSource;

        private HexCell[] m_Cells;
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
        public Texture2D noiseSource
        {
            get => m_NoiseSource;
            set => m_NoiseSource = value;
        }

        void Awake()
        {
            HexMetrics.noiseSource = m_NoiseSource;

            m_CellCountX = m_ChunkCountX * HexMetrics.chunkSizeX;
            m_CellCountZ = m_ChunkCountZ * HexMetrics.chunkSizeZ;

            CreateChunks();
            CreateCells();
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
                    chunk.transform.SetParent(transform);
                }
            }
        }

        void OnEnable()
        {
            HexMetrics.noiseSource = m_NoiseSource;
        }

        void Reset()
        {
            m_ChunkCountX = 6;
            m_ChunkCountZ = 6;
            m_CellPrefab = null;
            m_CellLabelPrefab = null;
            m_ChunkPrefab = null;
        }

        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            var coordinates = HexCoordinates.FromPosition(position);
            //Debug.LogFormat("touched at {0}", coordinates);
            int index = coordinates.x + coordinates.z * cellCountZ + coordinates.z / 2;
            return m_Cells[index];
        }
    }
}

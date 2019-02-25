using UnityEngine;
using UnityEngine.UI;

namespace FourEx
{
    public class HexGrid : MonoBehaviour
    {
        [SerializeField]
        private int m_Height;
        [SerializeField]
        private int m_Width;
        [SerializeField]
        private Color m_DefaultColor = Color.white;
        [SerializeField]
        private HexCell m_CellPrefab;
        [SerializeField]
        private Text m_CellLabelPrefab;

        private HexCell[] m_Cells;
        private Canvas m_GridCanvas;
        private HexMesh m_GridMesh;

        public int height
        {
            get { return m_Height; }
            //set { m_Height = value; }
        }
        public int width
        {
            get { return m_Width; }
            //set { m_Width = value; }
        }
        public Color defaultColor
        {
            get { return m_DefaultColor; }
            set { m_DefaultColor = value; }
        }
        public HexCell CellPrefab
        {
            get { return m_CellPrefab; }
            //set { m_CellPrefab = value; }
        }
        public Text cellLabelPrefab
        {
            get { return m_CellLabelPrefab; }
            set { m_CellLabelPrefab = value; }
        }

        void Awake()
        {
            m_GridCanvas = GetComponentInChildren<Canvas>();
            m_GridMesh = GetComponentInChildren<HexMesh>();
            m_Cells = new HexCell[height * width];
            for (int z = 0, i = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
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
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.color = defaultColor;

            Text label = Instantiate<Text>(m_CellLabelPrefab);
            label.rectTransform.SetParent(m_GridCanvas.transform, false);
            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
        }

        void Reset()
        {
            m_Height = 6;
            m_Width = 6;
            m_CellPrefab = null;
            m_CellLabelPrefab = null;
        }

        void Start()
        {
            m_GridMesh.Triangulate(m_Cells);
        }

        public void TouchCell(Vector3 position, Color color)
        {
            position = transform.InverseTransformPoint(position);
            var coordinates = HexCoordinates.FromPosition(position);
            Debug.LogFormat("touched at {0}", coordinates);
            int index = coordinates.x + coordinates.z * width + coordinates.z / 2;
            var cell = m_Cells[index];
            cell.color = color;
            m_GridMesh.Triangulate(m_Cells);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace FourEx
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        Mesh m_HexMesh;
        MeshCollider m_MeshCollider;
        List<Vector3> m_Vertices;
        List<int> m_Triangles;
        List<Color> m_Colors;

        void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_Vertices.Count;
            //Debug.LogFormat("adding {0} to vertex list at index {1}", v1, vertexIndex);
            m_Vertices.Add(v1);
            //Debug.LogFormat("adding {0} to vertex list at index {1}", v2, vertexIndex + 1);
            m_Vertices.Add(v2);
            //Debug.LogFormat("adding {0} to vertex list at index {1}", v3, vertexIndex + 2);
            m_Vertices.Add(v3);
            m_Triangles.Add(vertexIndex);
            m_Triangles.Add(vertexIndex + 1);
            m_Triangles.Add(vertexIndex + 2);
        }

        void Awake()
        {
            GetComponent<MeshFilter>().mesh = m_HexMesh = new Mesh();
            m_HexMesh.name = "Hex Mesh";
            m_Vertices = new List<Vector3>();
            m_Triangles = new List<int>();
            m_Colors = new List<Color>();
            m_MeshCollider = gameObject.AddComponent<MeshCollider>();
        }

        public void Triangulate(HexCell[] cells)
        {
            m_HexMesh.Clear();
            m_Vertices.Clear();
            m_Triangles.Clear();
            m_Colors.Clear();
            for (int i = 0; i < cells.Length; i++)
            {
                Triangulate(cells[i]);
            }
            m_HexMesh.vertices = m_Vertices.ToArray();
            m_HexMesh.colors = m_Colors.ToArray();
            m_HexMesh.triangles = m_Triangles.ToArray();
            m_HexMesh.RecalculateNormals();
            m_MeshCollider.sharedMesh = m_HexMesh;
        }

        void AddTriangleColor(Color color)
        {
            AddTriangleColor(color, color, color);
        }

        void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            m_Colors.Add(c1);
            m_Colors.Add(c2);
            m_Colors.Add(c3);
        }

        void Triangulate(HexCell cell)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, cell);
            }
        }

        void Triangulate(HexDirection direction, HexCell cell)
        {
            var center = cell.transform.localPosition;
            var v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            var v2 = center + HexMetrics.GetSecondSolidCorner(direction);

            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.color);

            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, v1, v2);
        }

        void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
        {
            var neighbor = cell.GetNeighbor(direction);
            if (neighbor == null)
                return;

            var bridge = HexMetrics.GetBridge(direction);
            var v3 = v1 + bridge;
            var v4 = v2 + bridge;
            v3.y = v4.y = neighbor.elevation * HexMetrics.elevationStep;

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
                TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
            else
            {
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(cell.color, neighbor.color);
            }

            var next_d = direction.Next();
            var nextNeighbor = cell.GetNeighbor(next_d);
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                var v5 = v2 + HexMetrics.GetBridge(next_d);
                v5.y = nextNeighbor.elevation * HexMetrics.elevationStep;
                AddTriangle(v2, v4, v5);
                AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
            }
        }

        void TriangulateEdgeTerraces(
            Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
            Vector3 endLeft, Vector3 endRight, HexCell endCell
        )
        {
            Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

            // first step
            AddQuad(beginLeft, beginRight, v3, v4);
            AddQuadColor(beginCell.color, c2);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c2;

                v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
                v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
                c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);

                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2);
            }

            // last step
            AddQuad(v3, v4, endLeft, endRight);
            AddQuadColor(c2, endCell.color);
        }

        void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            var index = m_Vertices.Count;
            m_Vertices.Add(v1);
            m_Vertices.Add(v2);
            m_Vertices.Add(v3);
            m_Vertices.Add(v4);
            m_Triangles.Add(index); // v1
            m_Triangles.Add(index + 2); // v3
            m_Triangles.Add(index + 1); // v2
            m_Triangles.Add(index + 1); // v2
            m_Triangles.Add(index + 2); // v3
            m_Triangles.Add(index + 3); // v4
        }

        void AddQuadColor(Color c1, Color c2)
        {
            AddQuadColor(c1, c1, c2, c2);
        }
        void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
        {
            m_Colors.Add(c1);
            m_Colors.Add(c2);
            m_Colors.Add(c3);
            m_Colors.Add(c4);
        }
    }
}
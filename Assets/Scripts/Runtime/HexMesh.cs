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
                if (cell.elevation <= neighbor.elevation)
                {
                    if (cell.elevation <= nextNeighbor.elevation)
                        TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                    else
                        TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
                else if (neighbor.elevation <= nextNeighbor.elevation)
                    TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
                else
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                //AddTriangle(v2, v4, v5);
                //AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
            }
        }

        void TriangulateCorner(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightCellType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightCellType == HexEdgeType.Slope)
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                else if (rightCellType == HexEdgeType.Flat)
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                else
                    TriangulateCornerTerracesCliff(
                        bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightCellType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                else
                    TriangulateCornerCliffTerraces(
                        bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.elevation < rightCell.elevation)
                    TriangulateCornerCliffTerraces(
                        right, rightCell, bottom, bottomCell, left, leftCell);
                else
                    TriangulateCornerTerracesCliff(
                        left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                AddTriangle(bottom, left, right);
                AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
            }
        }

        void TriangulateCornerTerracesCliff(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            float b = Mathf.Abs(1f / (rightCell.elevation - bottomCell.elevation));
            Vector3 boundary = Vector3.Lerp(bottom, right, b);
            Color boundaryColor = Color.Lerp(bottomCell.color, rightCell.color, b);

            TriangulateBoundaryTriangle(
                bottom, bottomCell, left, leftCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        }

        void TriangulateCornerCliffTerraces(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            float b = Mathf.Abs(1f / (leftCell.elevation - bottomCell.elevation));
            Vector3 boundary = Vector3.Lerp(bottom, left, b);
            Color boundaryColor = Color.Lerp(bottomCell.color, leftCell.color, b);

            TriangulateBoundaryTriangle(
                right, rightCell, bottom, bottomCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        }

        void TriangulateBoundaryTriangle(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 boundary, Color boundaryColor)
        {
            Vector3 v2 = HexMetrics.TerraceLerp(bottom, left, 1);
            Color c2 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, 1);

            AddTriangle(bottom, v2, boundary);
            AddTriangleColor(bottomCell.color, c2, boundaryColor);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;

                v2 = HexMetrics.TerraceLerp(bottom, left, i);
                c2 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, i);

                AddTriangle(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangle(v2, left, boundary);
            AddTriangleColor(c2, leftCell.color, boundaryColor);

        }

        void TriangulateCornerTerraces(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(bottom, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(bottom, right, 1);
            Color c3 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, 1);
            Color c4 = HexMetrics.TerraceLerp(bottomCell.color, rightCell.color, 1);

            // first step
            AddTriangle(bottom, v3, v4);
            AddTriangleColor(bottomCell.color, c3, c4);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;

                v3 = HexMetrics.TerraceLerp(bottom, left, i);
                v4 = HexMetrics.TerraceLerp(bottom, right, i);

                c3 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, i);
                c4 = HexMetrics.TerraceLerp(bottomCell.color, rightCell.color, i);

                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2, c3, c4);
            }

            // right step
            AddQuad(v3, v4, left, right);
            AddQuadColor(c3, c4, leftCell.color, rightCell.color);
        }

        void TriangulateEdgeTerraces(
            Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
            Vector3 endLeft, Vector3 endRight, HexCell endCell)
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
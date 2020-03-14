using System.Collections.Generic;
using UnityEngine;

namespace PlayfulSoftware.HexMaps
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        Mesh m_HexMesh;
        MeshCollider m_MeshCollider;
        static readonly List<Vector3> s_Vertices = new List<Vector3>();
        static readonly List<int> s_Triangles = new List<int>(); 
        static readonly List<Color> s_Colors = new List<Color>();

        void Awake()
        {
            GetComponent<MeshFilter>().mesh = m_HexMesh = new Mesh();
            m_HexMesh.name = "Hex Mesh";
            m_MeshCollider = gameObject.AddComponent<MeshCollider>();
        }

        void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            var index = s_Vertices.Count;
            s_Vertices.Add(Perturb(v1));
            s_Vertices.Add(Perturb(v2));
            s_Vertices.Add(Perturb(v3));
            s_Vertices.Add(Perturb(v4));
            s_Triangles.Add(index); // v1
            s_Triangles.Add(index + 2); // v3
            s_Triangles.Add(index + 1); // v2
            s_Triangles.Add(index + 1); // v2
            s_Triangles.Add(index + 2); // v3
            s_Triangles.Add(index + 3); // v4
        }

        void AddQuadColor(Color c1, Color c2)
        {
            AddQuadColor(c1, c1, c2, c2);
        }
        void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
        {
            s_Colors.Add(c1);
            s_Colors.Add(c2);
            s_Colors.Add(c3);
            s_Colors.Add(c4);
        }

        void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = s_Vertices.Count;
            s_Vertices.Add(Perturb(v1));
            s_Vertices.Add(Perturb(v2));
            s_Vertices.Add(Perturb(v3));
            s_Triangles.Add(vertexIndex);
            s_Triangles.Add(vertexIndex + 1);
            s_Triangles.Add(vertexIndex + 2);
        }

        void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = s_Vertices.Count;
            s_Vertices.Add(v1);
            s_Vertices.Add(v2);
            s_Vertices.Add(v3);
            s_Triangles.Add(vertexIndex);
            s_Triangles.Add(vertexIndex + 1);
            s_Triangles.Add(vertexIndex + 2);
        }

        void AddTriangleColor(Color color)
        {
            AddTriangleColor(color, color, color);
        }

        void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            s_Colors.Add(c1);
            s_Colors.Add(c2);
            s_Colors.Add(c3);
        }

        Vector3 Perturb(Vector3 position)
        {
            var sample = HexMetrics.SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
            //position.y += (sample.y * 2f - 1f) * HexMetrics.cellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
            return position;
        }

        public void Triangulate(HexCell[] cells)
        {
            m_HexMesh.Clear();
            s_Vertices.Clear();
            s_Triangles.Clear();
            s_Colors.Clear();
            for (int i = 0; i < cells.Length; i++)
            {
                Triangulate(cells[i]);
            }
            m_HexMesh.vertices = s_Vertices.ToArray();
            m_HexMesh.colors = s_Colors.ToArray();
            m_HexMesh.triangles = s_Triangles.ToArray();
            m_HexMesh.RecalculateNormals();
            m_MeshCollider.sharedMesh = m_HexMesh;
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
            var center = cell.position;
            var el = new EdgeVertices (
                center + HexMetrics.GetFirstSolidCorner(direction),
                center + HexMetrics.GetSecondSolidCorner(direction));

            /*
            var v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            var v2 = center + HexMetrics.GetSecondSolidCorner(direction);

            switch (subdivisions)
            {
                case 0:
                {
                    AddTriangle(center, v1, v2);
                    AddTriangleColor(cell.color);

                    break;
                }
                default:
                {
                    var sections = subdivisions + 1;

                    var e1 = Vector3.Lerp(v1, v2, 1f / (float)sections);
                    var e2 = e1;

                    // first triangle
                    AddTriangle(center, v1, e1);
                    AddTriangleColor(cell.color);

                    for (var i = 2; i < sections; i++)
                    {
                        e1 = e2;
                        e2 = Vector3.Lerp(v1, v2, (float)i / (float)sections);

                        AddTriangle(center, e1, e2);
                        AddTriangleColor(cell.color);
                    }

                    // last triangle
                    AddTriangle(center, e1, v2);
                    AddTriangleColor(cell.color);

                    break;
                }
            }
            */

            TriangulateEdgeFan(center, el, cell.color);

            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, el);
        }

        void TriangulateBoundaryTriangle(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 boundary, Color boundaryColor)
        {
            Vector3 v2 = Perturb(HexMetrics.TerraceLerp(bottom, left, 1));
            Color c2 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, 1);

            AddTriangleUnperturbed(Perturb(bottom), v2, boundary);
            AddTriangleColor(bottomCell.color, c2, boundaryColor);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;

                v2 = Perturb(HexMetrics.TerraceLerp(bottom, left, i));
                c2 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, i);

                AddTriangleUnperturbed(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangleUnperturbed(v2, Perturb(left), boundary);
            AddTriangleColor(c2, leftCell.color, boundaryColor);

        }

        void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices el)
        {
            var neighbor = cell.GetNeighbor(direction);
            if (neighbor == null)
                return;

            var bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.position.y - cell.position.y;
            var el2 = new EdgeVertices(
                el.v1 + bridge,
                el.v4 + bridge);

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
                TriangulateEdgeTerraces(el, cell, el2, neighbor);
            else
            {
                TriangulateEdgeStrip(el, cell.color, el2, neighbor.color);
            }

            var next_d = direction.Next();
            var nextNeighbor = cell.GetNeighbor(next_d);
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                var v5 = el.v4 + HexMetrics.GetBridge(next_d);
                v5.y = nextNeighbor.position.y;
                if (cell.elevation <= neighbor.elevation)
                {
                    if (cell.elevation <= nextNeighbor.elevation)
                        TriangulateCorner(el.v4, cell, el2.v4, neighbor, v5, nextNeighbor);
                    else
                        TriangulateCorner(v5, nextNeighbor, el.v4, cell, el2.v4, neighbor);
                }
                else if (neighbor.elevation <= nextNeighbor.elevation)
                    TriangulateCorner(el2.v4, neighbor, v5, nextNeighbor, el.v4, cell);
                else
                    TriangulateCorner(v5, nextNeighbor, el.v4, cell, el2.v4, neighbor);
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
            Vector3 boundary = Vector3.Lerp(Perturb(bottom), Perturb(right), b);
            Color boundaryColor = Color.Lerp(bottomCell.color, rightCell.color, b);

            TriangulateBoundaryTriangle(
                bottom, bottomCell, left, leftCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        }

        void TriangulateCornerCliffTerraces(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            float b = Mathf.Abs(1f / (leftCell.elevation - bottomCell.elevation));
            Vector3 boundary = Vector3.Lerp(Perturb(bottom), Perturb(left), b);
            Color boundaryColor = Color.Lerp(bottomCell.color, leftCell.color, b);

            TriangulateBoundaryTriangle(
                right, rightCell, bottom, bottomCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
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

        void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
        {
            AddTriangle(center, edge.v1, edge.v2);
            AddTriangleColor(color);
            AddTriangle(center, edge.v2, edge.v3);
            AddTriangleColor(color);
            AddTriangle(center, edge.v3, edge.v4);
            AddTriangleColor(color);
        }

        void TriangulateEdgeFan(Vector3 center, EdgeVertexList edge, Color color)
        {
            foreach (var pairs in edge.pairs)
            {
                AddTriangle(center, pairs.Item1, pairs.Item2);
                AddTriangleColor(color);
            }
        }

        void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
        {
            AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            AddQuadColor(c1, c2);
            AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            AddQuadColor(c1, c2);
            AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            AddQuadColor(c1, c2);
        }

        void TriangulateEdgeStrip(EdgeVertexList e1, Color c1, EdgeVertexList e2, Color c2)
        {
            using (var p1 = e1.pairs.GetEnumerator())
            using (var p2 = e2.pairs.GetEnumerator())
            {
                if (!(p1.MoveNext() && p2.MoveNext()))
                    return;

                do {
                    var it1 = p1.Current;
                    var it2 = p2.Current;
                    AddQuad(it1.Item1, it1.Item2, it2.Item1, it2.Item2);
                    AddQuadColor(c1, c2);
                } while (p1.MoveNext() && p2.MoveNext());
            }
        }

        void TriangulateEdgeTerraces(
            EdgeVertices begin, HexCell beginCell,
            EdgeVertices end, HexCell endCell)
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

            // first step
            TriangulateEdgeStrip(begin, beginCell.color, e2, c2);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                EdgeVertices e1 = e2;
                Color c1 = c2;

                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);

                TriangulateEdgeStrip(e1, c1, e2, c2);
            }

            // last step
            TriangulateEdgeStrip(e2, c2, end, endCell.color);
        }
    }
}
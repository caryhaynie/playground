using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexGridChunk : MonoBehaviour
    {
        private HexCell[] m_Cells;
        private Canvas m_GridCanvas;

        public HexMesh terrain;
        public HexMesh rivers;
        public HexMesh roads;

        void Awake()
        {
            m_GridCanvas = GetComponentInChildren<Canvas>();

            m_Cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
            ShowUI(false);
        }

        public void AddCell(int index, HexCell cell)
        {
            m_Cells[index] = cell;
            cell.chunk = this;
            cell.transform.SetParent(transform, false);
            cell.uiRect.SetParent(m_GridCanvas.transform, false);
        }

        public void Refresh()
        {
            enabled = true;
        }

        public void ShowUI(bool visible)
        {
            m_GridCanvas.gameObject.SetActive(visible);
        }

        void LateUpdate()
        {
            Triangulate();
            enabled = false;
        }

        Vector2 GetRoadInterpolators(HexDirection dir, HexCell cell)
        {
            if (cell.HasRoadThroughEdge(dir))
                return  new Vector2(0.5f, 0.5f);
            else
            {
                var interp = new Vector2();
                interp.x = cell.HasRoadThroughEdge(dir.Previous()) ? 0.5f : 0.25f;
                interp.y = cell.HasRoadThroughEdge(dir.Next()) ? 0.5f : 0.25f;
                return interp;
            }
        }

        void Triangulate()
        {
            if (!terrain)
                return;
            terrain.Clear();
            rivers.Clear();
            roads.Clear();
            for (int i = 0; i < m_Cells.Length; i++)
            {
                Triangulate(m_Cells[i]);
            }
            terrain.Apply();
            rivers.Apply();
            roads.Apply();
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

            if (cell.hasRiver)
            {
                if (cell.HasRiverThroughEdge(direction))
                {
                    el.v3.y = cell.streamBedY;
                    if (cell.hasRiverBeginOrEnd)
                        TriangulateWithRiverBeginOrEnd(direction, cell, center, el);
                    else
                        TriangulateWithRiver(direction, cell, center, el);
                }
                else
                {
                    TriangulateAdjacentToRiver(direction, cell, center, el);
                }
            }
            else
                TriangulateWithoutRiver(direction, cell, center, el);

            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, el);
        }

        void TriangulateAdjacentToRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
        {
            if (cell.HasRoads)
            {
                TriangulateRoadAdjacentToRiver(dir, cell, center, e);
            }
            if (cell.HasRiverThroughEdge(dir.Next()))
            {
                if (cell.HasRiverThroughEdge(dir.Previous()))
                {
                    center += HexMetrics.GetSolidEdgeMiddle(dir)
                              * (HexMetrics.innerToOuter * 0.5f);
                }
                else if (cell.HasRiverThroughEdge(dir.Previous2()))
                {
                    center += HexMetrics.GetFirstSolidCorner(dir) * 0.25f;
                }
            }
            else if (cell.HasRiverThroughEdge(dir.Previous()) &&
                     cell.HasRiverThroughEdge(dir.Next2()))
            {
                center += HexMetrics.GetSecondSolidCorner(dir) * 0.25f;
            }
            var m = new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center, e.v5, 0.5f));

            TriangulateEdgeStrip(m, cell.color, e, cell.color);
            TriangulateEdgeFan(center, m, cell.color);
        }

        void TriangulateBoundaryTriangle(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 boundary, Color boundaryColor)
        {
            Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(bottom, left, 1));
            Color c2 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, 1);

            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(bottom), v2, boundary);
            terrain.AddTriangleColor(bottomCell.color, c2, boundaryColor);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;

                v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(bottom, left, i));
                c2 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, i);

                terrain.AddTriangleUnperturbed(v1, v2, boundary);
                terrain.AddTriangleColor(c1, c2, boundaryColor);
            }

            terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
            terrain.AddTriangleColor(c2, leftCell.color, boundaryColor);

        }

        void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices el)
        {
            var neighbor = cell.GetNeighbor(direction);
            if (!neighbor)
                return;

            var bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.position.y - cell.position.y;
            var el2 = new EdgeVertices(
                el.v1 + bridge,
                el.v5 + bridge);

            if (cell.HasRiverThroughEdge(direction))
            {
                el2.v3.y = neighbor.streamBedY;
                TriangulateRiverQuad(
                    el.v2, el.v4, el2.v2, el2.v4,
                    cell.riverSurfaceY, neighbor.riverSurfaceY,
                    cell.hasIncomingRiver && cell.incomingRiver == direction);
            }

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
                TriangulateEdgeTerraces(el, cell, el2, neighbor, cell.HasRoadThroughEdge(direction));
            else
            {
                TriangulateEdgeStrip(el, cell.color, el2, neighbor.color, cell.HasRoadThroughEdge(direction));
            }

            var next_d = direction.Next();
            var nextNeighbor = cell.GetNeighbor(next_d);
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                var v5 = el.v5 + HexMetrics.GetBridge(next_d);
                v5.y = nextNeighbor.position.y;
                if (cell.elevation <= neighbor.elevation)
                {
                    if (cell.elevation <= nextNeighbor.elevation)
                        TriangulateCorner(el.v5, cell, el2.v5, neighbor, v5, nextNeighbor);
                    else
                        TriangulateCorner(v5, nextNeighbor, el.v5, cell, el2.v5, neighbor);
                }
                else if (neighbor.elevation <= nextNeighbor.elevation)
                    TriangulateCorner(el2.v5, neighbor, v5, nextNeighbor, el.v5, cell);
                else
                    TriangulateCorner(v5, nextNeighbor, el.v5, cell, el2.v5, neighbor);
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
                terrain.AddTriangle(bottom, left, right);
                terrain.AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
            }
        }

        void TriangulateCornerTerracesCliff(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            float b = Mathf.Abs(1f / (rightCell.elevation - bottomCell.elevation));
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(bottom), HexMetrics.Perturb(right), b);
            Color boundaryColor = Color.Lerp(bottomCell.color, rightCell.color, b);

            TriangulateBoundaryTriangle(
                bottom, bottomCell, left, leftCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        }

        void TriangulateCornerCliffTerraces(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            float b = Mathf.Abs(1f / (leftCell.elevation - bottomCell.elevation));
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(bottom), HexMetrics.Perturb(left), b);
            Color boundaryColor = Color.Lerp(bottomCell.color, leftCell.color, b);

            TriangulateBoundaryTriangle(
                right, rightCell, bottom, bottomCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
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
            terrain.AddTriangle(bottom, v3, v4);
            terrain.AddTriangleColor(bottomCell.color, c3, c4);

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

                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadColor(c1, c2, c3, c4);
            }

            // right step
            terrain.AddQuad(v3, v4, left, right);
            terrain.AddQuadColor(c3, c4, leftCell.color, rightCell.color);
        }

        void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
        {
            terrain.AddTriangle(center, edge.v1, edge.v2);
            terrain.AddTriangleColor(color);
            terrain.AddTriangle(center, edge.v2, edge.v3);
            terrain.AddTriangleColor(color);
            terrain.AddTriangle(center, edge.v3, edge.v4);
            terrain.AddTriangleColor(color);
            terrain.AddTriangle(center, edge.v4, edge.v5);
            terrain.AddTriangleColor(color);
        }

        void TriangulateEdgeFan(Vector3 center, EdgeVertexList edge, Color color)
        {
            foreach (var pairs in edge.pairs)
            {
                terrain.AddTriangle(center, pairs.Item1, pairs.Item2);
                terrain.AddTriangleColor(color);
            }
        }

        void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2, bool hasRoad = false)
        {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
            terrain.AddQuadColor(c1, c2);

            if (hasRoad)
                TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
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
                    terrain.AddQuad(it1.Item1, it1.Item2, it2.Item1, it2.Item2);
                    terrain.AddQuadColor(c1, c2);
                } while (p1.MoveNext() && p2.MoveNext());
            }
        }

        void TriangulateEdgeTerraces(
            EdgeVertices begin, HexCell beginCell,
            EdgeVertices end, HexCell endCell, bool hasRoad = false)
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

            // first step
            TriangulateEdgeStrip(begin, beginCell.color, e2, c2, hasRoad);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                EdgeVertices e1 = e2;
                Color c1 = c2;

                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);

                TriangulateEdgeStrip(e1, c1, e2, c2, hasRoad);
            }

            // last step
            TriangulateEdgeStrip(e2, c2, end, endCell.color, hasRoad);
        }

        void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reversed)
        {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            rivers.AddQuad(v1, v2, v3, v4);
            if (reversed)
                rivers.AddQuadUV(1f, 0f, 0.8f - v , 0.6f - v);
            else
                rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
        }

        void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed) =>
            TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, reversed);

        void TriangulateRoadAdjacentToRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
        {
            var hasRoadThroughEdge = cell.HasRoadThroughEdge(dir);
            var previousHasRiver = cell.HasRiverThroughEdge(dir.Previous());
            var nextHasRiver = cell.HasRiverThroughEdge(dir.Next());
            var interpolators = GetRoadInterpolators(dir, cell);
            var roadCenter = center;

            if (cell.hasRiverBeginOrEnd)
            {
                roadCenter += HexMetrics.GetSolidEdgeMiddle(
                                  cell.riverBeginOrEndDirection.Opposite()
                                  ) * (1f / 3f);
            }
            else if (cell.incomingRiver == cell.outgoingRiver.Opposite())
            {
                Vector3 corner;
                if (previousHasRiver)
                {
                    if (!hasRoadThroughEdge &&
                        !cell.HasRoadThroughEdge(dir.Next()))
                        return;
                    corner = HexMetrics.GetSecondSolidCorner(dir);
                }
                else
                {
                    if (!hasRoadThroughEdge &&
                        !cell.HasRoadThroughEdge(dir.Previous()))
                        return;
                    corner = HexMetrics.GetFirstSolidCorner(dir);
                }

                roadCenter += corner * 0.5f;
                center += corner * 0.25f;
            }
            else if (cell.incomingRiver == cell.outgoingRiver.Previous())
            {
                roadCenter -= HexMetrics.GetSecondCorner(cell.incomingRiver) * 0.2f;
            }
            else if (cell.incomingRiver == cell.outgoingRiver.Next())
            {
                roadCenter -= HexMetrics.GetFirstCorner(cell.incomingRiver) * 0.2f;
            }
            else if (previousHasRiver && nextHasRiver)
            {
                if (!hasRoadThroughEdge)
                    return;
                var offset = HexMetrics.GetSolidEdgeMiddle(dir) * HexMetrics.innerToOuter;
                roadCenter += offset * 0.7f;
                center += offset * 0.5f;
            }
            else
            {
                HexDirection middle;
                if (previousHasRiver)
                    middle = dir.Next();
                else if (nextHasRiver)
                    middle = dir.Previous();
                else
                    middle = dir;
                if (!cell.HasRoadThroughEdge(middle) &&
                    !cell.HasRoadThroughEdge(middle.Previous()) &&
                    !cell.HasRoadThroughEdge(middle.Next()))
                    return;
                roadCenter += HexMetrics.GetSolidEdgeMiddle(middle) * 0.25f;
            }
            var mL = Vector3.Lerp(roadCenter, e.v1, interpolators.x);
            var mR = Vector3.Lerp(roadCenter, e.v5, interpolators.y);
            TriangulateRoad(roadCenter, mL, mR, e, hasRoadThroughEdge);
            if (previousHasRiver)
                TriangulateRoadEdge(roadCenter, center, mL);
            if (nextHasRiver)
                TriangulateRoadEdge(roadCenter, mR, center);
        }

        void TriangulateRoad(Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughCellEdge)
        {
            if (hasRoadThroughCellEdge)
            {
                var mC = Vector3.Lerp(mL, mR, 0.5f);
                TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4);
                roads.AddTriangle(center, mL, mC);
                roads.AddTriangle(center, mC, mR);
                roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
                roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
            }
            else
            {
                TriangulateRoadEdge(center, mL, mR);
            }
        }

        void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR)
        {
            roads.AddTriangle(center, mL, mR);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f,0f), new Vector2(0f, 0f));
        }

        void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
        {
            roads.AddQuad(v1, v2, v4, v5);
            roads.AddQuad(v2, v3, v5, v6);
            roads.AddQuadUV(0f, 1f, 0f, 0f);
            roads.AddQuadUV(1f, 0f, 0f, 0f);
        }

        void TriangulateWithoutRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
        {
            TriangulateEdgeFan(center, e, cell.color);

            if (cell.HasRoads)
            {
                var interpolators = GetRoadInterpolators(dir, cell);
                TriangulateRoad(center,
                    Vector3.Lerp(center, e.v1, interpolators.x),
                    Vector3.Lerp(center, e.v5, interpolators.y),
                    e, cell.HasRoadThroughEdge(dir));
            }
        }

        void TriangulateWithRiver (
            HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
        )
        {
            Vector3 centerL, centerR;
            if (cell.HasRiverThroughEdge(direction.Opposite()))
            {
                centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
                centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
            }
            else if (cell.HasRiverThroughEdge(direction.Next()))
            {
                centerL = center;
                centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
            }
            else if (cell.HasRiverThroughEdge(direction.Previous()))
            {
                centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
                centerR = center;
            }
            else if (cell.HasRiverThroughEdge(direction.Next2()))
            {
                centerL = center;
                centerR = center + HexMetrics.GetSolidEdgeMiddle(direction.Next())
                    * (0.5f * HexMetrics.innerToOuter);
            }
            else
            {
                centerL = center + HexMetrics.GetSolidEdgeMiddle(direction.Previous())
                    * (0.5f * HexMetrics.innerToOuter);
                centerR = center;
            }

            center = Vector3.Lerp(centerL, centerR, 0.5f);

            var m = new EdgeVertices(
                Vector3.Lerp(centerL, e.v1, 0.5f),
                Vector3.Lerp(centerR, e.v5, 0.5f),
                1f / 6f);

            m.v3.y = center.y = e.v3.y;
            TriangulateEdgeStrip(m, cell.color, e, cell.color);

            terrain.AddTriangle(centerL, m.v1, m.v2);
            terrain.AddTriangleColor(cell.color);
            terrain.AddQuad(centerL, center, m.v2, m.v3);
            terrain.AddQuadColor(cell.color);
            terrain.AddQuad(center, centerR, m.v3, m.v4);
            terrain.AddQuadColor(cell.color);
            terrain.AddTriangle(centerR, m.v4, m.v5);
            terrain.AddTriangleColor(cell.color);

            var reversed = cell.incomingRiver == direction;

            TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.riverSurfaceY, 0.4f, reversed);
            TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.riverSurfaceY, 0.6f, reversed);
        }

        void TriangulateWithRiverBeginOrEnd(
            HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
        )
        {
            var m = new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center,e.v5, 0.5f));

            m.v3.y = e.v3.y;
            TriangulateEdgeStrip(m, cell.color, e, cell.color);
            TriangulateEdgeFan(center, m, cell.color);

            bool reversed = cell.hasIncomingRiver;
            TriangulateRiverQuad(
                m.v2, m.v4, e.v2, e.v4, cell.riverSurfaceY, 0.6f, reversed);
            center.y = m.v2.y = m.v4.y = cell.riverSurfaceY;
            rivers.AddTriangle(center, m.v2, m.v4);
            if (reversed)
                rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(1f, 0.2f), new Vector2(0f, 0.2f));
            else
                rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(0f, 0.6f), new Vector2(1f, 0.6f));
        }
    }
}
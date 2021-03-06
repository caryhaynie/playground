using System;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;

    [CustomEditor(typeof(HexGridChunk))]
    sealed class HexGridChunkEditor : Editor
    {
    }
#endif // UNITY_EDITOR
    [ExecuteAlways]
    public sealed class HexGridChunk : MonoBehaviour
    {
        [SerializeField]
        private HexCell[] m_Cells;
        [SerializeField]
        private Canvas m_GridCanvas;

        //public ChunkMeshData meshData;

        static Color color1 = new Color(1f, 0f, 0f);
        static Color color2 = new Color(0f, 1f, 0f);
        static Color color3 = new Color(0f, 0f, 1f);

        public HexMesh estuaries;
        public HexMesh rivers;
        public HexMesh roads;
        public HexMesh terrain;
        public HexMesh walls;
        public HexMesh water;
        public HexMesh waterShore;

        public HexFeatureManager features;

        void Awake()
        {
            if (!m_GridCanvas)
                m_GridCanvas = GetComponentInChildren<Canvas>();
            if (m_Cells.IsNullOrEmpty())
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
#if UNITY_EDITOR
            if (!ShouldPerformChunkUpdate())
                return;
            if (!Application.IsPlaying(gameObject))
                Undo.RecordObject(gameObject, "Chunk Updated");
#endif // UNITY_EDITOR
            if (HasValidCellArray())
                Triangulate();
            enabled = false;
        }

        bool CanAddBridge(HexCell cell, HexDirection dir)
        {
            if (cell.incomingRiver != dir.Next())
                return false;
            return cell.HasRoadThroughEdge(dir.Next2()) || cell.HasRoadThroughEdge(dir.Opposite());
        }

        bool CanAddFeature(HexCell cell, HexDirection dir)
        {
            return !cell.isUnderWater && !cell.HasRoadThroughEdge(dir);
        }

        bool CanAddFeatureToCenter(HexCell cell)
        {
            return !cell.hasRiver && !cell.HasRoads;
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

        bool HasValidCellArray()
        {
            if (m_Cells == null)
                return false;
            if (m_Cells.Length == 0)
                return false;
            foreach (var cell in m_Cells)
                if (!cell)
                    return false;
            return true;
        }

#if UNITY_EDITOR
        bool ShouldPerformChunkUpdate()
            => Application.IsPlaying(gameObject) || StageUtility.GetCurrentStageHandle() == StageUtility.GetMainStageHandle();
#endif // UNITY_EDITOR

        void Triangulate()
        {
            if (!terrain)
                return;
            estuaries.Clear();
            terrain.Clear();
            rivers.Clear();
            roads.Clear();
            water.Clear();
            waterShore.Clear();

            features.Clear();

            foreach (var cell in m_Cells)
            {
                Triangulate(cell);
            }

            estuaries.Apply();
            terrain.Apply();
            rivers.Apply();
            roads.Apply();
            water.Apply();
            waterShore.Apply();

            features.Apply();
        }

        void Triangulate(HexCell cell)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                Triangulate(d, cell);
            if (!cell.isUnderWater)
            {
                if (CanAddFeatureToCenter(cell))
                    features.AddFeature(cell, cell.position);
                if (cell.isSpecial)
                    features.AddSpecialFeature(cell, cell.position);
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
            {
                TriangulateWithoutRiver(direction, cell, center, el);
                if (CanAddFeature(cell, direction))
                {
                    // offset the feature slightly from the center towards
                    // the directional edge.
                    var pos = (center + el.v1 + el.v5) * (1f / 3f);
                    features.AddFeature(cell, pos);
                }
            }

            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, el);

            if (cell.isUnderWater)
                TriangulateWater(direction, cell, center);
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

            TriangulateEdgeStrip(m, color1, cell.terrainTypeIndex, e, color1, cell.terrainTypeIndex);
            TriangulateEdgeFan(center, m, cell.terrainTypeIndex);

            if (CanAddFeature(cell, dir))
            {
                // offset the feature slightly from the center towards
                // the directional edge.
                var pos = (center + e.v1 + e.v5) * (1f / 3f);
                features.AddFeature(cell, pos);
            }
        }

        void TriangulateBoundaryTriangle(
            Vector3 bottom, Color beginColor,
            Vector3 left, Color leftColor,
            Vector3 boundary, Color boundaryColor, Vector3 types)
        {
            Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(bottom, left, 1));
            Color c2 = HexMetrics.TerraceLerp(beginColor, leftColor, 1);

            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(bottom), v2, boundary);
            terrain.AddTriangleColor(beginColor, c2, boundaryColor);
            terrain.AddTriangleTerrainTypes(types);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;

                v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(bottom, left, i));
                c2 = HexMetrics.TerraceLerp(beginColor, leftColor, i);

                terrain.AddTriangleUnperturbed(v1, v2, boundary);
                terrain.AddTriangleColor(c1, c2, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);
            }

            terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
            terrain.AddTriangleColor(c2, leftColor, boundaryColor);
            terrain.AddTriangleTerrainTypes(types);

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

            var hasRiver = cell.HasRiverThroughEdge(direction);
            var hasRoad = cell.HasRoadThroughEdge(direction);

            if (hasRiver)
            {
                el2.v3.y = neighbor.streamBedY;

                if (!cell.isUnderWater)
                {
                    if (!neighbor.isUnderWater)
                    {
                        TriangulateRiverQuad(
                            el.v2, el.v4, el2.v2, el2.v4,
                            cell.riverSurfaceY, neighbor.riverSurfaceY,
                            cell.hasIncomingRiver && cell.incomingRiver == direction);
                    }
                    else if (cell.elevation > neighbor.waterLevel)
                    {
                        TriangulateWaterfallInWater(el.v2, el.v4, el2.v2, el2.v4,
                            cell.riverSurfaceY, neighbor.riverSurfaceY, neighbor.waterSurfaceY);
                    }
                }
                else if (!neighbor.isUnderWater && neighbor.elevation > cell.waterLevel)
                {
                    TriangulateWaterfallInWater(el2.v4, el2.v2, el.v4, el.v2,
                        neighbor.riverSurfaceY, cell.riverSurfaceY, cell.waterSurfaceY);
                }
            }

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
                TriangulateEdgeTerraces(el, cell, el2, neighbor, hasRoad);
            else
            {
                TriangulateEdgeStrip(el, color1, cell.terrainTypeIndex, el2, color2, neighbor.terrainTypeIndex, hasRoad);
            }

            features.AddWall(el, cell, el2, neighbor, hasRiver, hasRoad);

            var next_d = direction.Next();
            var nextNeighbor = cell.GetNeighbor(next_d);

            if (direction <= HexDirection.E && nextNeighbor)
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
                terrain.AddTriangleColor(color1, color2, color3);
                Vector3 types;
                types.x = bottomCell.terrainTypeIndex;
                types.y = leftCell.terrainTypeIndex;
                types.z = rightCell.terrainTypeIndex;
                terrain.AddTriangleTerrainTypes(types);
            }
            features.AddWall(bottom, bottomCell, left, leftCell, right, rightCell);
        }

        void TriangulateCornerTerracesCliff(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            float b = Mathf.Abs(1f / (rightCell.elevation - bottomCell.elevation));
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(bottom), HexMetrics.Perturb(right), b);
            Color boundaryColor = Color.Lerp(color1, color3, b);

            Vector3 types;
            types.x = bottomCell.terrainTypeIndex;
            types.y = leftCell.terrainTypeIndex;
            types.z = rightCell.terrainTypeIndex;

            TriangulateBoundaryTriangle(
                bottom, color1, left, color2, boundary, boundaryColor, types);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, color2, right, color3, boundary, boundaryColor, types);
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleColor(color2, color3, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);
            }
        }

        void TriangulateCornerCliffTerraces(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            float b = Mathf.Abs(1f / (leftCell.elevation - bottomCell.elevation));
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(bottom), HexMetrics.Perturb(left), b);
            Color boundaryColor = Color.Lerp(color1, color2, b);

            Vector3 types;
            types.x = bottomCell.terrainTypeIndex;
            types.y = leftCell.terrainTypeIndex;
            types.z = rightCell.terrainTypeIndex;

            TriangulateBoundaryTriangle(
                right, color3, bottom, color1, boundary, boundaryColor, types);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(
                    left, color2, right, color3, boundary, boundaryColor, types);
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleColor(color2, color3, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);
            }
        }

        void TriangulateCornerTerraces(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(bottom, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(bottom, right, 1);
            Color c3 = HexMetrics.TerraceLerp(color1, color2, 1);
            Color c4 = HexMetrics.TerraceLerp(color1, color3, 1);
            Vector3 types;
            types.x = bottomCell.terrainTypeIndex;
            types.y = leftCell.terrainTypeIndex;
            types.z = rightCell.terrainTypeIndex;

            // first step
            terrain.AddTriangle(bottom, v3, v4);
            terrain.AddTriangleColor(color1, c3, c4);
            terrain.AddTriangleTerrainTypes(types);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;

                v3 = HexMetrics.TerraceLerp(bottom, left, i);
                v4 = HexMetrics.TerraceLerp(bottom, right, i);

                c3 = HexMetrics.TerraceLerp(color1, color2, i);
                c4 = HexMetrics.TerraceLerp(color1, color3, i);

                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadColor(c1, c2, c3, c4);
                terrain.AddQuadTerrainTypes(types);
            }

            // right step
            terrain.AddQuad(v3, v4, left, right);
            terrain.AddQuadColor(c3, c4, color2, color3);
            terrain.AddQuadTerrainTypes(types);
        }

        void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float type)
        {
            terrain.AddTriangle(center, edge.v1, edge.v2);
            terrain.AddTriangle(center, edge.v2, edge.v3);
            terrain.AddTriangle(center, edge.v3, edge.v4);
            terrain.AddTriangle(center, edge.v4, edge.v5);

            terrain.AddTriangleColor(color1);
            terrain.AddTriangleColor(color1);
            terrain.AddTriangleColor(color1);
            terrain.AddTriangleColor(color1);

            var types = Vector3.one * type;

            terrain.AddTriangleTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);
        }

        void TriangulateEdgeStrip(
            EdgeVertices e1, Color c1, float type1,
            EdgeVertices e2, Color c2, float type2, bool hasRoad = false)
        {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
            terrain.AddQuadColor(c1, c2);

            var types = new Vector3(type1, type2, type1);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);

            if (hasRoad)
                TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
        }

        void TriangulateEdgeTerraces(
            EdgeVertices begin, HexCell beginCell,
            EdgeVertices end, HexCell endCell, bool hasRoad = false)
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color c2 = HexMetrics.TerraceLerp(color1, color2, 1);
            float t1 = beginCell.terrainTypeIndex;
            float t2 = endCell.terrainTypeIndex;

            // first step
            TriangulateEdgeStrip(begin, color1, t1, e2, c2, t2, hasRoad);

            for (int i = 2; i < HexMetrics.terracedSteps; i++)
            {
                EdgeVertices e1 = e2;
                Color c1 = c2;

                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(color1, color2, i);

                TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2, hasRoad);
            }

            // last step
            TriangulateEdgeStrip(e2, c2, t1, end, color2, t2, hasRoad);
        }

        void TriangulateEstuary(EdgeVertices e1, EdgeVertices e2, bool incomingRiver)
        {
            waterShore.AddTriangle(e2.v1, e1.v2, e1.v1);
            waterShore.AddTriangle(e2.v5, e1.v5, e1.v4);
            waterShore.AddTriangleUV(new Vector2(0f, 1f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f));
            waterShore.AddTriangleUV(new Vector2(0f, 1f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f));

            estuaries.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
            estuaries.AddTriangle(e1.v3, e2.v2, e2.v4);
            estuaries.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);

            estuaries.AddQuadUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 0f));
            estuaries.AddTriangleUV(
                new Vector2(0f, 0f), new Vector2(0f, 01), new Vector2(0f, 1f));
            estuaries.AddQuadUV(
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f));

            if (incomingRiver)
            {
                estuaries.AddQuadUV2(
                    new Vector2(1.5f, 1f), new Vector2(0.7f, 1.15f),
                    new Vector2(1f, 0.8f), new Vector2(0.5f, 1.1f));
                estuaries.AddTriangleUV2(
                    new Vector2(0.5f, 1.1f), new Vector2(1f, 0.8f), new Vector2(0f, 0.8f));
                estuaries.AddQuadUV2(
                    new Vector2(0.5f, 1.1f), new Vector2(0.3f, 1.15f),
                    new Vector2(0f, 0.8f), new Vector2(-0.5f, 1f));
            }
            else
            {
                estuaries.AddQuadUV2(
                    new Vector2(-0.5f, -0.2f), new Vector2(0.3f, -0.35f),
                    new Vector2(0f, 0.8f), new Vector2(0.5f, -0.3f));
                estuaries.AddTriangleUV2(
                    new Vector2(0.5f, -0.3f), new Vector2(0f, 0f), new Vector2(1f, 0f));
                estuaries.AddQuadUV2(
                    new Vector2(0.5f, -0.3f), new Vector2(0.7f, -0.35f),
                    new Vector2(1f, 0f), new Vector2(1.5f,  -0.2f));
            }
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
                if (CanAddBridge(cell, dir))
                    features.AddBridge(roadCenter, center - corner * 0.5f);
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
                var offset = HexMetrics.GetSolidEdgeMiddle(middle);
                roadCenter += offset * 0.25f;
                if (dir == middle &&
                    cell.HasRoadThroughEdge(dir.Opposite()))
                    features.AddBridge(
                        roadCenter,
                        center - offset * (HexMetrics.innerToOuter * 0.7f));
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

        void TriangulateWater(HexDirection dir, HexCell cell, Vector3 center)
        {
            center.y = cell.waterSurfaceY;

            var neighbor = cell.GetNeighbor(dir);
            if (neighbor && !neighbor.isUnderWater)
                TriangulateWaterShore(dir, cell, neighbor, center);
            else
                TriangulateOpenWater(dir, cell, neighbor, center);
        }

        void TriangulateOpenWater(HexDirection dir, HexCell cell, HexCell neighbor, Vector3 center)
        {
            var c1 = center + HexMetrics.GetFirstWaterCorner(dir);
            var c2 = center + HexMetrics.GetSecondWaterCorner(dir);

            water.AddTriangle(center, c1, c2);

            if (dir <= HexDirection.SE && neighbor)
            {
                var bridge = HexMetrics.GetWaterBridge(dir);
                var e1 = c1 + bridge;
                var e2 = c2 + bridge;

                water.AddQuad(c1, c2, e1, e2);

                if (dir <= HexDirection.E)
                {
                    var nextNeighbor = cell.GetNeighbor(dir.Next());
                    if (!nextNeighbor || !nextNeighbor.isUnderWater)
                        return;
                    water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(dir.Next()));
                }
            }
        }

        void TriangulateWaterfallInWater(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float waterY)
        {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            v1 = HexMetrics.Perturb(v1);
            v2 = HexMetrics.Perturb(v2);
            v3 = HexMetrics.Perturb(v3);
            v4 = HexMetrics.Perturb(v4);
            float t = (waterY - y2) / (y1 - y2);
            v3 = Vector3.Lerp(v3, v1, t);
            v4 = Vector3.Lerp(v4, v2, t);
            rivers.AddQuadUnperturbed(v1, v2, v3, v4);
            rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
        }

        void TriangulateWaterShore(HexDirection dir, HexCell cell, HexCell neighbor, Vector3 center)
        {
            var e1 = new EdgeVertices(
                center + HexMetrics.GetFirstWaterCorner(dir),
                center + HexMetrics.GetSecondWaterCorner(dir));

            water.AddTriangle(center, e1.v1, e1.v2);
            water.AddTriangle(center, e1.v2, e1.v3);
            water.AddTriangle(center, e1.v3, e1.v4);
            water.AddTriangle(center, e1.v4, e1.v5);

            var center2 = neighbor.position;
            center2.y = center.y;

            var e2 = new EdgeVertices(
                center2 + HexMetrics.GetSecondSolidCorner(dir.Opposite()),
                center2 + HexMetrics.GetFirstSolidCorner(dir.Opposite()));

            if (cell.HasRiverThroughEdge(dir))
                TriangulateEstuary(e1, e2, cell.incomingRiver == dir);
            else
            {
                waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
                waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
                waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
                waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            }

            var nextNeighbor = cell.GetNeighbor(dir.Next());
            if (nextNeighbor)
            {
                var v3 = nextNeighbor.position + (nextNeighbor.isUnderWater
                    ? HexMetrics.GetFirstWaterCorner(dir.Previous())
                    : HexMetrics.GetFirstSolidCorner(dir.Previous()));
                v3.y = center.y;
                waterShore.AddTriangle(e1.v5, e2.v5, v3);
                waterShore.AddTriangleUV(
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, nextNeighbor.isUnderWater ? 0f : 1f));
            }
        }

        void TriangulateWithoutRiver(HexDirection dir, HexCell cell, Vector3 center, EdgeVertices e)
        {
            TriangulateEdgeFan(center, e, cell.terrainTypeIndex);

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
            TriangulateEdgeStrip(m, color1, cell.terrainTypeIndex, e, color1, cell.terrainTypeIndex);

            terrain.AddTriangle(centerL, m.v1, m.v2);
            //terrain.AddTriangleColor(cell.color);
            terrain.AddQuad(centerL, center, m.v2, m.v3);
            //terrain.AddQuadColor(cell.color);
            terrain.AddQuad(center, centerR, m.v3, m.v4);
            //terrain.AddQuadColor(cell.color);
            terrain.AddTriangle(centerR, m.v4, m.v5);
            //terrain.AddTriangleColor(cell.color);

            terrain.AddTriangleColor(color1);
            terrain.AddQuadColor(color1);
            terrain.AddQuadColor(color1);
            terrain.AddTriangleColor(color1);

            var types = Vector3.one * cell.terrainTypeIndex;
            terrain.AddTriangleTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);

            if (!cell.isUnderWater)
            {
                var reversed = cell.incomingRiver == direction;

                TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.riverSurfaceY, 0.4f, reversed);
                TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.riverSurfaceY, 0.6f, reversed);
            }
        }

        void TriangulateWithRiverBeginOrEnd(
            HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
        )
        {
            var m = new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center,e.v5, 0.5f));

            m.v3.y = e.v3.y;
            TriangulateEdgeStrip(m, color1, cell.terrainTypeIndex, e, color1, cell.terrainTypeIndex);
            TriangulateEdgeFan(center, m, cell.terrainTypeIndex);

            if (!cell.isUnderWater)
            {
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
}
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexFeatureManager : MonoBehaviour
    {
        public HexFeatureCollection[] farmCollections;
        public HexFeatureCollection[] plantCollections;
        public HexFeatureCollection[] urbanCollections;

        public HexMesh walls;

        Transform m_Container;

        public void AddFeature(HexCell cell, Vector3 position)
        {
            var hash = HexMetrics.SampleHashGrid(position);
            var prefab = PickPrefab(urbanCollections, cell.urbanLevel, hash.a, hash.d);
            var otherPrefab = PickPrefab(farmCollections, cell.farmLevel, hash.b, hash.d);
            float usedHash = hash.a;
            if (prefab)
            {
                if (otherPrefab && hash.b < usedHash)
                {
                    prefab = otherPrefab;
                    usedHash = hash.b;
                }
            }
            else if (otherPrefab)
            {
                prefab = otherPrefab;
                usedHash = hash.b;
            }
            otherPrefab = PickPrefab(plantCollections, cell.plantLevel, hash.c, hash.d);
            if (prefab)
            {
                if (otherPrefab && hash.c < usedHash)
                    prefab = otherPrefab;
            }
            else if (otherPrefab)
                prefab = otherPrefab;
            else
                return;

            var instance = Instantiate(prefab, m_Container, false);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = HexMetrics.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        }

        public void Apply()
        {
            walls.Apply();
        }

        public void AddWall(
            EdgeVertices near, HexCell nearCell,
            EdgeVertices far, HexCell farCell,
            bool hasRiver, bool hasRoad)
        {
            if (!CanBuildWall(nearCell, farCell)) return;

            AddWallSegment(near.v1, far.v1, near.v2, far.v2);
            if (hasRiver || hasRoad)
            {
                AddWallCap(near.v2, far.v2);
                AddWallCap(far.v4, near.v4);
            }
            else
            {
                AddWallSegment(near.v2, far.v2, near.v3, far.v3);
                AddWallSegment(near.v3, far.v3, near.v4, far.v4);
            }
            AddWallSegment(near.v4, far.v4, near.v5, far.v5);
        }

        public void AddWall(
            Vector3 c1, HexCell cell1,
            Vector3 c2, HexCell cell2,
            Vector3 c3, HexCell cell3)
        {
            if (cell1.walled)
            {
                if (cell2.walled)
                {
                    if (!cell3.walled)
                        AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
                }
                else if (cell3.walled)
                    AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
                else
                    AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
            }
            else if (cell2.walled)
            {
                if (cell3.walled)
                    AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
                else
                    AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
            }
            else if (cell3.walled)
                AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
        }

        void AddWallCap(Vector3 near, Vector3 far)
        {
            near = HexMetrics.Perturb(near);
            far = HexMetrics.Perturb(far);

            var center = HexMetrics.WallLerp(near, far);
            var thickness = HexMetrics.WallThicknessOffset(near, far);

            Vector3 v1, v2, v3, v4;

            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = center.y + HexMetrics.wallHeight;
            walls.AddQuadUnperturbed(v1, v2, v3, v4);
        }

        void AddWallSegment(Vector3 pivot, HexCell pivotCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            if (pivotCell.isUnderWater)
                return;

            var hasLeftWall = !leftCell.isUnderWater &&
                            pivotCell.GetEdgeType(leftCell) != HexEdgeType.Cliff;
            var hasRightWall = !rightCell.isUnderWater &&
                               pivotCell.GetEdgeType(rightCell) != HexEdgeType.Cliff;
            if (hasLeftWall)
            {
                if (hasRightWall)
                    AddWallSegment(pivot, left, pivot, right);
                else if (leftCell.elevation < rightCell.elevation)
                    AddWallWedge(pivot, left, right);
                else
                    AddWallCap(pivot, left);
            }
            else if (hasRightWall)
                if (rightCell.elevation < leftCell.elevation)
                    AddWallWedge(right, pivot, left);
                else
                    AddWallCap(right, pivot);
        }

        void AddWallSegment(Vector3 nearLeft, Vector3 farLeft, Vector3 nearRight, Vector3 farRight)
        {
            // pre-perturb our vertices
            nearLeft = HexMetrics.Perturb(nearLeft);
            nearRight = HexMetrics.Perturb(nearRight);
            farLeft = HexMetrics.Perturb(farLeft);
            farRight = HexMetrics.Perturb(farRight);

            var left = HexMetrics.WallLerp(nearLeft, farLeft);
            var right = HexMetrics.WallLerp(nearRight, farRight);

            var leftThicknessOffset =
                HexMetrics.WallThicknessOffset(nearLeft, farLeft);
            var rightThicknessOffset =
                HexMetrics.WallThicknessOffset(nearRight, farRight);

            var leftTop = left.y + HexMetrics.wallHeight;
            var rightTop = right.y + HexMetrics.wallHeight;

                Vector3 v1, v2, v3, v4;
            v1 = v3 = left - leftThicknessOffset;
            v2 = v4 = right - rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            walls.AddQuadUnperturbed(v1, v2, v3, v4);

            Vector3 t1 = v3, t2 = v4;

            // make the wall double-sided
            v1 = v3 = left + leftThicknessOffset;
            v2 = v4 = right + rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            walls.AddQuadUnperturbed(v2, v1, v4, v3);

            // add a wall top too!
            walls.AddQuadUnperturbed(t1, t2, v3, v4);
        }

        void AddWallWedge(Vector3 near, Vector3 far, Vector3 point)
        {
            near = HexMetrics.Perturb(near);
            far = HexMetrics.Perturb(far);
            point = HexMetrics.Perturb(point);

            var center = HexMetrics.WallLerp(near, far);
            var thickness = HexMetrics.WallThicknessOffset(near, far);

            Vector3 v1, v2, v3, v4;
            var pointTop = point;
            point.y = center.y;

            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = pointTop.y = center.y + HexMetrics.wallHeight;
            walls.AddQuadUnperturbed(v1, point, v3, pointTop);
            walls.AddQuadUnperturbed(point, v2, pointTop, v4);
            walls.AddTriangleUnperturbed(pointTop, v3, v4);
        }

        bool CanBuildWall(HexCell nearCell, HexCell farCell)
        {
            if (nearCell.walled == farCell.walled) return false;
            if (nearCell.isUnderWater || farCell.isUnderWater) return false;
            return nearCell.GetEdgeType(farCell) != HexEdgeType.Cliff;
        }

        public void Clear()
        {
            if (m_Container)
                GameObjectUtility.SafelyDeleteGameObject(m_Container.gameObject);
            m_Container = new GameObject("FeaturesContainer").transform;
            m_Container.SetParent(transform, false);

            walls.Clear();
        }

        Transform PickPrefab(
            HexFeatureCollection[] collection,
            int level, float hash, float choice)
        {
            Transform prefab = null;
            if (level <= 0)
                return prefab;
            var thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for (var i = 0; i < thresholds.Length; i++)
            {
                if (hash < thresholds[i])
                {
                    prefab = collection[i].Pick(choice);
                    break;
                }
            }

            return prefab;
        }
    }
}
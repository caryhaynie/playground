using System;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(HexCell.RiverState))]
    class RiverStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dir = (HexDirection)property.FindPropertyRelative("direction").enumValueIndex;
            var enabled = property.FindPropertyRelative("exists").boolValue;
            position = EditorGUI.PrefixLabel(position, label);
            GUI.Label(position, enabled ? $"{dir}" : "None");
        }
    }
#endif // UNITY_EDITOR
    public sealed class HexCell : MonoBehaviour
    {
        [Serializable]
        public struct RiverState
        {
            public HexDirection direction;
            public bool exists;
        }

        public HexCoordinates coordinates;

        [HideInInspector] public HexGridChunk chunk;
        [HideInInspector] public RectTransform uiRect;

        [SerializeField] HexCell[] m_Neighbors;
        [SerializeField] bool[] m_Roads;

        [SerializeField] Color m_Color;
        [SerializeField] int m_Elevation = Int32.MinValue;

        [Header("Water")]
        [SerializeField] int m_WaterLevel;
        [SerializeField] RiverState m_IncomingRiver;
        [SerializeField] RiverState m_OutgoingRiver;

        [Header("Features")]
        [SerializeField] int m_FarmLevel;
        [SerializeField] int m_PlantLevel;
        [SerializeField] int m_UrbanLevel;

        public Color color
        {
            get => m_Color;
            set
            {
                if (m_Color == value)
                    return;
                m_Color = value;

                Refresh();
            }
        }

        public int elevation
        {
            get => m_Elevation;
            set
            {
                if (m_Elevation == value)
                    return;
                m_Elevation = value;

                // verify rivers
                RemoveRiversIfInvalid();

                // verify roads
                RemoveRoadsIfInvalid();

                // Update Transform
                var pos = transform.localPosition;
                pos.y = m_Elevation * HexMetrics.elevationStep;
                pos.y +=
                    (HexMetrics.SampleNoise(pos).y * 2f - 1f) *
                    HexMetrics.elevationPerturbStrength;
                transform.localPosition = pos;

                // Update UI Transform
                var uiPosition = uiRect.localPosition;
                uiPosition.z = -pos.y;
                uiRect.localPosition = uiPosition;
                Refresh();
            }
        }

        public bool hasIncomingRiver => m_IncomingRiver.exists;
        public bool hasOutgoingRiver => m_OutgoingRiver.exists;
        public HexDirection incomingRiver => m_IncomingRiver.direction;
        public bool isUnderWater => m_WaterLevel > m_Elevation;
        public HexDirection outgoingRiver => m_OutgoingRiver.direction;

        public bool hasRiver => hasIncomingRiver || hasOutgoingRiver;
        public bool hasRiverBeginOrEnd => hasIncomingRiver != hasOutgoingRiver;

        public Vector3 position => transform.localPosition;

        public HexDirection riverBeginOrEndDirection => hasIncomingRiver ? incomingRiver : outgoingRiver;

        public float riverSurfaceY => (elevation + HexMetrics.waterSurfaceElevationOffset) * HexMetrics.elevationStep;
        public float streamBedY => (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;

        public int farmLevel
        {
            get => m_FarmLevel;
            set
            {
                if (m_FarmLevel == value)
                    return;
                m_FarmLevel = value;
                RefreshSelfOnly();
            }
        }

        public int plantLevel
        {
            get => m_PlantLevel;
            set
            {
                if (m_PlantLevel == value)
                    return;
                m_PlantLevel = value;
                RefreshSelfOnly();
            }
        }

        public int urbanLevel
        {
            get => m_UrbanLevel;
            set
            {
                if (m_UrbanLevel == value)
                    return;
                m_UrbanLevel = value;
                RefreshSelfOnly();
            }
        }

        public int waterLevel
        {
            get => m_WaterLevel;
            set
            {
                if (m_WaterLevel == value)
                    return;
                m_WaterLevel = value;
                RemoveRiversIfInvalid();
                Refresh();
            }
        }

        public float waterSurfaceY => (waterLevel + HexMetrics.waterSurfaceElevationOffset) * HexMetrics.elevationStep;

#if UNITY_EDITOR
        void Reset()
        {
            m_Neighbors = new HexCell[6];
            m_Roads = new bool[6];
        }
#endif // UNITY_EDITOR

        public void AddRoad(HexDirection dir)
        {
            if (!m_Roads[(int) dir]
                && !HasRiverThroughEdge(dir)
                && GetElevationDifference(dir) <= 1)
            {
                SetRoad((int)dir, true);
            }
        }

        public HexEdgeType GetEdgeType(HexCell otherCell)
        {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return GetEdgeType(GetNeighbor(direction));
        }

        public int GetElevationDifference(HexDirection dir)
        {
            var neighbor = GetNeighbor(dir);
            return !neighbor ? 0 : Mathf.Abs(elevation - neighbor.elevation);
        }

        public HexCell GetNeighbor(HexDirection direction)
        {
            return m_Neighbors[(int) direction];
        }

        public bool HasRiverThroughEdge(HexDirection dir)
        {
            return hasIncomingRiver && incomingRiver == dir ||
                   hasOutgoingRiver && outgoingRiver == dir;
        }

        public bool HasRoads
        {
            get
            {
                for (int i = 0; i < m_Roads.Length; i++)
                    if (m_Roads[i])
                        return true;
                return false;
            }
        }

        public bool HasRoadThroughEdge(HexDirection dir) => m_Roads[(int) dir];

        bool IsValidRiverDestination(HexCell neighbor) =>
            neighbor && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);

        public void RemoveIncomingRiver()
        {
            if (!hasIncomingRiver)
                return;
            m_IncomingRiver.exists = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(incomingRiver);
            if (neighbor)
            {
                neighbor.m_OutgoingRiver.exists = false;
                neighbor.RefreshSelfOnly();
            }
        }

        public void RemoveOutgoingRiver()
        {
            if (!hasOutgoingRiver)
                return;
            m_OutgoingRiver.exists = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(outgoingRiver);
            if (neighbor)
            {
                neighbor.m_IncomingRiver.exists = false;
                neighbor.RefreshSelfOnly();
            }
        }

        public void RemoveRiver()
        {
            RemoveIncomingRiver();
            RemoveOutgoingRiver();
        }

        void RemoveRiversIfInvalid()
        {
            if (hasOutgoingRiver)
            {
                var neighbor = GetNeighbor(outgoingRiver);
                if (neighbor && elevation < neighbor.elevation)
                    RemoveOutgoingRiver();
            }

            if (hasIncomingRiver)
            {
                var neighbor = GetNeighbor(incomingRiver);
                if (neighbor && elevation > neighbor.elevation)
                    RemoveIncomingRiver();
            }
        }

        public void RemoveRoads()
        {
            for (int i = 0; i < m_Neighbors.Length; i++)
            {
                if (m_Roads[i])
                {
                    SetRoad(i, false);
                }
            }
        }

        void RemoveRoadsIfInvalid()
        {
            for (int i = 0; i < m_Roads.Length; i++)
            {
                if (m_Roads[i] && GetElevationDifference((HexDirection) i) > 1)
                {
                    SetRoad(i, false);
                }
            }
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            m_Neighbors[(int) direction] = cell;
            cell.m_Neighbors[(int) direction.Opposite()] = this;
        }

        public void SetOutgoingRiver(HexDirection dir)
        {
            // nothing to do; already have an outgoing river in this direction.
            if (hasOutgoingRiver && outgoingRiver == dir)
                return;

            var neighbor = GetNeighbor(dir);
            // rivers can't flow up-hill, so bail if neighbor elevation is higher
            // than us.
            if (!IsValidRiverDestination(neighbor))
                return;

            // remove the old outgoing river, if it exists.
            // additionally, if the neighbor was previously
            // an incoming river, delete that too.
            RemoveOutgoingRiver();
            if (hasIncomingRiver && incomingRiver == dir)
                RemoveIncomingRiver();

            // actually update the river state now, and refresh.
            m_OutgoingRiver.exists = true;
            m_OutgoingRiver.direction = dir;

            // update our neighbor too.
            if (!neighbor)
                return;
            neighbor.RemoveIncomingRiver();
            neighbor.m_IncomingRiver.exists = true;
            neighbor.m_IncomingRiver.direction = dir.Opposite();

            SetRoad((int)dir, false);
        }

        void Refresh()
        {
            if (!chunk) return;
            chunk.Refresh();
            RefreshNeighborsIfInDifferentChunks();
        }

        void RefreshSelfOnly()
        {
            if (!chunk)
                return;
            chunk.Refresh();
        }

        void RefreshNeighborsIfInDifferentChunks()
        {
            for (int i = 0; i < m_Neighbors.Length; i++)
            {
                var nChunk = m_Neighbors[i]?.chunk;
                if (nChunk && nChunk != chunk)
                    nChunk.Refresh();
            }
        }

        void SetRoad(int index, bool state)
        {
            m_Roads[index] = state;
            m_Neighbors[index].m_Roads[(int) ((HexDirection) index).Opposite()] = state;
            m_Neighbors[index].RefreshSelfOnly();
            RefreshSelfOnly();
        }
    }
}
using System;
using System.IO;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(HexCell.RiverState))]
    sealed class RiverStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dir = (HexDirection)property.FindPropertyRelative("direction").enumValueIndex;
            var enabled = property.FindPropertyRelative("exists").boolValue;
            EditorGUI.LabelField(position, label, new GUIContent(enabled ? $"{dir}" : "None"));
        }
    }

    [CustomEditor(typeof(HexCell))]
    sealed class HexCellEditor : Editor
    {
        private SerializedProperty m_CoordinatesProp;
        private SerializedProperty m_ChunkProp;
        private SerializedProperty m_UIRectProp;
        private SerializedProperty m_NeighborsProp;
        private SerializedProperty m_RoadsProp;
        private SerializedProperty m_ElevationProp;
        private SerializedProperty m_WaterLevelProp;

        private Lazy<GUIStyle> centeredLabel =
            new Lazy<GUIStyle>(() =>
            {
                var style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                return style;
            });

        // overrides the base class with the usefully typed version.
        private new HexCell target => (HexCell) base.target;

        void OnEnable()
        {
            m_CoordinatesProp = serializedObject.FindProperty("coordinates");
            m_ChunkProp = serializedObject.FindProperty("chunk");
            m_UIRectProp = serializedObject.FindProperty("uiRect");
            m_NeighborsProp = serializedObject.FindProperty("m_Neighbors");
            m_RoadsProp = serializedObject.FindProperty("m_Roads");
            m_ElevationProp = serializedObject.FindProperty("m_Elevation");
            m_WaterLevelProp = serializedObject.FindProperty("m_WaterLevel");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_CoordinatesProp);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ChunkProp);
                EditorGUILayout.PropertyField(m_UIRectProp);
                m_NeighborsProp.isExpanded = true;
                EditorGUILayout.PropertyField(m_NeighborsProp);
            }

            LevelControl(m_ElevationProp);
            LevelControl(m_WaterLevelProp);

            bool shouldRefresh = serializedObject.hasModifiedProperties;
            serializedObject.ApplyModifiedProperties();
            if (shouldRefresh)
                target.Refresh();
        }

        void LevelControl(SerializedProperty prop)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel($"{prop.displayName}");

                if (GUILayout.Button("-"))
                {
                    prop.intValue--;
                }
                GUILayout.Label($"{prop.intValue}", centeredLabel.Value);
                if (GUILayout.Button("+"))
                {
                    prop.intValue++;
                }
            }
        }

        void OnSceneGUI()
        {

        }
    }
#endif // UNITY_EDITOR
    [ExecuteAlways]
    public sealed class HexCell : MonoBehaviour
    {
        [Serializable]
        internal struct RiverState
        {
            public HexDirection direction;
            public bool exists;

            public void Load(BinaryReader reader)
            {
                var data = reader.ReadByte();
                if (data >= 128)
                {
                    exists = true;
                    direction = (HexDirection) (data - 128);
                }
                else
                    exists = false;
            }

            public void Save(BinaryWriter writer)
            {
                if (exists)
                    writer.Write((byte) (direction + 128));
                else
                    writer.Write((byte)0);
            }
        }

        public HexCoordinates coordinates;

        [HideInInspector] public HexGridChunk chunk;
        [HideInInspector] public RectTransform uiRect;

        [SerializeField] HexCell[] m_Neighbors;
        [SerializeField] bool[] m_Roads;

        [SerializeField] int m_Elevation = Int32.MinValue;
        [SerializeField] int m_TerrainTypeIndex;
        [SerializeField] int m_WaterLevel;

        [SerializeField] RiverState m_IncomingRiver;
        [SerializeField] RiverState m_OutgoingRiver;

        [SerializeField] int m_FarmLevel;
        [SerializeField] int m_PlantLevel;
        [SerializeField] int m_SpecialIndex;
        [SerializeField] int m_UrbanLevel;
        [SerializeField] bool m_Walled;

        public Color color
        {
            get => HexMetrics.GetTerrainColor(m_TerrainTypeIndex);
        }

        public int elevation
        {
            get => m_Elevation;
            set => SetElevationInternal(value);
        }

        public bool hasIncomingRiver => m_IncomingRiver.exists;
        public bool hasOutgoingRiver => m_OutgoingRiver.exists;
        public HexDirection incomingRiver => m_IncomingRiver.direction;
        public bool isSpecial => m_SpecialIndex > 0;
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

        public int specialIndex
        {
            get => m_SpecialIndex;
            set
            {
                if (m_SpecialIndex == value || hasRiver)
                    return;
                m_SpecialIndex = value;
                RemoveRoads();
                RefreshSelfOnly();
            }
        }

        public int terrainTypeIndex
        {
            get => m_TerrainTypeIndex;
            set
            {
                if (m_TerrainTypeIndex == value)
                    return;
                m_TerrainTypeIndex = value;
                Refresh();
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
            set => SetWaterLevelInternal(value);
        }

        public float waterSurfaceY => (waterLevel + HexMetrics.waterSurfaceElevationOffset) * HexMetrics.elevationStep;

        public bool walled
        {
            get => m_Walled;
            set
            {
                if (m_Walled == value)
                    return;
                m_Walled = value;
                Refresh();
            }
        }

#if UNITY_EDITOR
        void Reset()
        {
            m_Neighbors = new HexCell[6];
            m_Roads = new bool[6];
        }
#endif // UNITY_EDITOR

        public void AddRoad(HexDirection dir)
        {
            if (CanAddRoad(dir))
                SetRoad((int)dir, true);
        }

        bool CanAddRoad(HexDirection dir)
        {
            if (m_Roads[(int) dir]) return false;
            if (HasRiverThroughEdge(dir)) return false;
            if (GetElevationDifference(dir) > 1) return false;
            return !isSpecial && !GetNeighbor(dir).isSpecial;
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

        void RefreshPosition()
        {
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
        }

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

        internal void SetElevationInternal(int value)
        {
            if (m_Elevation == value)
                return;
            m_Elevation = value;

            // verify rivers
            RemoveRiversIfInvalid();

            // verify roads
            RemoveRoadsIfInvalid();

            RefreshPosition();
            Refresh();
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
            m_SpecialIndex = 0;

            // update our neighbor too.
            if (!neighbor)
                return;
            neighbor.RemoveIncomingRiver();
            neighbor.m_IncomingRiver.exists = true;
            neighbor.m_IncomingRiver.direction = dir.Opposite();
            neighbor.m_SpecialIndex = 0;

            SetRoad((int)dir, false);
        }

        internal void SetWaterLevelInternal(int value)
        {
            if (m_WaterLevel == value)
                return;
            m_WaterLevel = value;
            RemoveRiversIfInvalid();
            Refresh();
        }

        internal void Refresh()
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

        public void Load(BinaryReader reader)
        {
            m_TerrainTypeIndex = reader.ReadByte();
            m_Elevation = reader.ReadByte();
            RefreshPosition();
            m_WaterLevel = reader.ReadByte();
            m_UrbanLevel = reader.ReadByte();
            m_FarmLevel = reader.ReadByte();
            m_PlantLevel = reader.ReadByte();
            m_SpecialIndex = reader.ReadByte();
            m_Walled = reader.ReadBoolean();

            m_IncomingRiver.Load(reader);
            m_OutgoingRiver.Load(reader);

            int roadFlags = reader.ReadByte();
            for (int i = 0; i < m_Roads.Length; i++)
                m_Roads[i] = (roadFlags & (1 << i)) != 0;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)m_TerrainTypeIndex);
            writer.Write((byte)m_Elevation);
            writer.Write((byte)m_WaterLevel);
            writer.Write((byte)m_UrbanLevel);
            writer.Write((byte)m_FarmLevel);
            writer.Write((byte)m_PlantLevel);
            writer.Write((byte)m_SpecialIndex);
            writer.Write(m_Walled);

            m_IncomingRiver.Save(writer);
            m_OutgoingRiver.Save(writer);

            int roadFlags = 0;
            for (int i = 0; i < m_Roads.Length; i++)
                if (m_Roads[i])
                    roadFlags |= 1 << i;
            writer.Write((byte)roadFlags);
        }
    }
}
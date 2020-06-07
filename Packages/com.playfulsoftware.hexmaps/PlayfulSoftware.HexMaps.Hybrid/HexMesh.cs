using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    #if UNITY_EDITOR
    using UnityEditor;
    #endif // UNITY_EDITOR
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class HexMesh : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField]
        private MeshUsageOptions m_Options;
        #pragma warning restore 0649

        public bool useCollider => m_Options.HasFlag(MeshUsageOptions.Collider);
        public bool useColors => m_Options.HasFlag(MeshUsageOptions.Colors);
        public bool useUVCoordinates => m_Options.HasFlag(MeshUsageOptions.UV);
        public bool useUV2Coordinates => m_Options.HasFlag(MeshUsageOptions.UV2);
        public bool useTerrainTypes => m_Options.HasFlag(MeshUsageOptions.TerrainType);

        [HideInInspector,SerializeField]
        Mesh m_HexMesh;
        [HideInInspector,SerializeField]
        MeshCollider m_MeshCollider;

        [NonSerialized] private List<Vector3> m_Vertices;
        [NonSerialized] private List<int> m_Triangles;
        [NonSerialized] private List<Color> m_Colors;
        [NonSerialized] private List<Vector2> m_UVs;
        [NonSerialized] private List<Vector2> m_UV2s;
        [NonSerialized] private List<Vector3> m_TerrainTypes;

        public bool LogMeshStatsOnAwake;

        #if UNITY_EDITOR
        void OnValidate()
        {

        }
        #endif // UNITY_EDITOR

        void Awake()
        {
            if (LogMeshStatsOnAwake)
                LogMeshStats();
            if (!m_HexMesh)
                SetMeshInternal(new Mesh(), "Hex Mesh");
            if (useCollider && !m_MeshCollider)
                m_MeshCollider = gameObject.AddComponent<MeshCollider>();
        }

        internal void SetMeshInternal(Mesh mesh, string meshName = null)
        {
            if (Application.IsPlaying(gameObject))
                GetComponent<MeshFilter>().mesh = m_HexMesh = new Mesh();
            else
                GetComponent<MeshFilter>().sharedMesh = m_HexMesh = new Mesh();
            if (!string.IsNullOrEmpty(meshName))
                m_HexMesh.name = meshName;
        }

        public void Apply()
        {
            m_HexMesh.SetVertices(m_Vertices);
            ListPool<Vector3>.Add(m_Vertices);
            if (useColors)
            {
                m_HexMesh.SetColors(m_Colors);
                ListPool<Color>.Add(m_Colors);
            }

            if (useUVCoordinates)
            {
                m_HexMesh.SetUVs(0, m_UVs);
                ListPool<Vector2>.Add(m_UVs);
            }

            if (useUV2Coordinates)
            {
                m_HexMesh.SetUVs(1, m_UV2s);
                ListPool<Vector2>.Add(m_UV2s);
            }

            if (useTerrainTypes)
            {
                m_HexMesh.SetUVs(2, m_TerrainTypes);
                ListPool<Vector3>.Add(m_TerrainTypes);
            }

            m_HexMesh.SetTriangles(m_Triangles, 0);
            ListPool<int>.Add(m_Triangles);
            m_HexMesh.RecalculateNormals();
            if (useCollider)
                m_MeshCollider.sharedMesh = m_HexMesh;
#if UNITY_EDITOR
            if (!Application.IsPlaying(gameObject))
            {
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(m_HexMesh);
            }
#endif // UNITY_EDITOR

        }

        public void Clear()
        {
            m_HexMesh.Clear();
            m_Vertices = ListPool<Vector3>.Get();
            m_Triangles = ListPool<int>.Get();
            if (useColors)
                m_Colors = ListPool<Color>.Get();
            if (useUVCoordinates)
                m_UVs = ListPool<Vector2>.Get();
            if (useUV2Coordinates)
                m_UV2s = ListPool<Vector2>.Get();
            if (useTerrainTypes)
                m_TerrainTypes = ListPool<Vector3>.Get();
        }

        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            var index = m_Vertices.Count;
            m_Vertices.Add(HexMetrics.Perturb(v1));
            m_Vertices.Add(HexMetrics.Perturb(v2));
            m_Vertices.Add(HexMetrics.Perturb(v3));
            m_Vertices.Add(HexMetrics.Perturb(v4));
            m_Triangles.Add(index); // v1
            m_Triangles.Add(index + 2); // v3
            m_Triangles.Add(index + 1); // v2
            m_Triangles.Add(index + 1); // v2
            m_Triangles.Add(index + 2); // v3
            m_Triangles.Add(index + 3); // v4
        }

        public void AddQuadUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
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

        public void AddQuadColor(Color c)
        {
            AddQuadColor(c, c, c, c);
        }

        public void AddQuadColor(Color c1, Color c2)
        {
            AddQuadColor(c1, c1, c2, c2);
        }

        public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
        {
            if (!useColors)
                return;
            m_Colors.Add(c1);
            m_Colors.Add(c2);
            m_Colors.Add(c3);
            m_Colors.Add(c4);
        }

        public void AddTriangleTerrainTypes(Vector3 types)
        {
            if (!useTerrainTypes)
                return;
            m_TerrainTypes.Add(types);
            m_TerrainTypes.Add(types);
            m_TerrainTypes.Add(types);
        }

        public void AddQuadTerrainTypes(Vector3 types)
        {
            if (!useTerrainTypes)
                return;
            m_TerrainTypes.Add(types);
            m_TerrainTypes.Add(types);
            m_TerrainTypes.Add(types);
            m_TerrainTypes.Add(types);
        }

        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_Vertices.Count;
            m_Vertices.Add(HexMetrics.Perturb(v1));
            m_Vertices.Add(HexMetrics.Perturb(v2));
            m_Vertices.Add(HexMetrics.Perturb(v3));
            m_Triangles.Add(vertexIndex);
            m_Triangles.Add(vertexIndex + 1);
            m_Triangles.Add(vertexIndex + 2);
        }

        public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_Vertices.Count;
            m_Vertices.Add(v1);
            m_Vertices.Add(v2);
            m_Vertices.Add(v3);
            m_Triangles.Add(vertexIndex);
            m_Triangles.Add(vertexIndex + 1);
            m_Triangles.Add(vertexIndex + 2);
        }

        public void AddTriangleColor(Color color)
        {
            AddTriangleColor(color, color, color);
        }

        public void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            if (!useColors)
                return;
            m_Colors.Add(c1);
            m_Colors.Add(c2);
            m_Colors.Add(c3);
        }

        public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            if (!useUVCoordinates)
                return;
            m_UVs.Add(uv1);
            m_UVs.Add(uv2);
            m_UVs.Add(uv3);
        }

        public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
        {
            if (!useUVCoordinates)
                return;
            m_UVs.Add(uv1);
            m_UVs.Add(uv2);
            m_UVs.Add(uv3);
            m_UVs.Add(uv4);
        }

        public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
        {
            AddQuadUV(
                new Vector2(uMin, vMin),
                new Vector2(uMax, vMin),
                new Vector2(uMin, vMax),
                new Vector2(uMax, vMax));
        }

        public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            if (!useUV2Coordinates)
                return;
            m_UV2s.Add(uv1);
            m_UV2s.Add(uv2);
            m_UV2s.Add(uv3);
        }

        public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
        {
            if (!useUV2Coordinates)
                return;
            m_UV2s.Add(uv1);
            m_UV2s.Add(uv2);
            m_UV2s.Add(uv3);
            m_UV2s.Add(uv4);
        }

        public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax)
        {
            AddQuadUV2(
                new Vector2(uMin, vMin),
                new Vector2(uMax, vMin),
                new Vector2(uMin, vMax),
                new Vector2(uMax, vMax));
        }

        void LogMeshStats()
        {
            if (!m_HexMesh)
            {
                DebugHelper.LogErrorNoStacktrace("No HexMesh!");
                return;
            }

            var chunkName = transform.parent.name;
            DebugHelper.LogNoStacktrace($"{name} for {chunkName}");
            DebugHelper.LogNoStacktrace($"\tVert Count: {m_HexMesh.vertexCount}");
        }
    }
}
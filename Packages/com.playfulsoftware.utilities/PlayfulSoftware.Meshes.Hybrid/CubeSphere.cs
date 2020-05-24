using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayfulSoftware.Meshes.Hybrid
{
    using static MeshUtility;
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class CubeSphere : MonoBehaviour
    {
        public int gridSize;
        public int radius;

        private Color32[] m_Colors;
        private Vector3[] m_Normals;
        private Vector3[] m_Vertices;
        // Start is called before the first frame update
        void Awake()
        {
            Generate();
        }

        void CreateColliders()
        {
            gameObject.AddComponent<SphereCollider>();
        }

        int CreateBottomFace(int[] triangles, int t, int ring)
        {
            int v = 1;
            int vMid = m_Vertices.Length - (gridSize - 1) * (gridSize - 1);
            t = SetQuadNoAlloc(triangles, t, ring - 1, vMid, 0, 1);
            for (int x = 1; x < gridSize - 1; x++, v++, vMid++) {
                t = SetQuadNoAlloc(triangles, t, vMid, vMid + 1, v, v + 1);
            }
            t = SetQuadNoAlloc(triangles, t, vMid, v + 2, v, v + 1);

            int vMin = ring - 2;
            vMid -= gridSize - 2;
            int vMax = v + 2;

            for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++) {
                t = SetQuadNoAlloc(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
                for (int x = 1; x < gridSize - 1; x++, vMid++) {
                    t = SetQuadNoAlloc(
                        triangles, t,
                        vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
                }
                t = SetQuadNoAlloc(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuadNoAlloc(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++) {
                t = SetQuadNoAlloc(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }
            t = SetQuadNoAlloc(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        int CreateTopFace(int[] triangles, int t, int ring)
        {
            var v = ring * gridSize;
            for (var x = 0; x < gridSize - 1; x++, v++)
            {
                t = SetQuadNoAlloc(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }

            t = SetQuadNoAlloc(triangles, t, v, v + 1, v + ring - 1, v + 2);

            var vMin = ring * (gridSize + 1) - 1;
            var vMid = vMin + 1;
            var vMax = v + 2;

            for (var z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuadNoAlloc(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
                for (var x = 1; x < gridSize - 1; x++, vMid++)
                {
                    t = SetQuadNoAlloc(triangles, t, vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
                }
                t = SetQuadNoAlloc(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
            }

            var vTop = vMin - 2;
            t = SetQuadNoAlloc(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (var x = 1; x < gridSize - 1; x++, vTop--, vMid++)
                t = SetQuadNoAlloc(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            t = SetQuadNoAlloc(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        void CreateTriangles(Mesh mesh)
        {
            var xQuads = (gridSize * gridSize) * 12;
            var yQuads = (gridSize * gridSize) * 12;
            var zQuads = (gridSize * gridSize) * 12;

            //var triangles = new int[quads * 6];
            var trianglesX = new int[xQuads];
            var trianglesY = new int[yQuads];
            var trianglesZ = new int[zQuads];

            var ring = (gridSize + gridSize) * 2;
            //var t = 0;
            var tX = 0;
            var tY = 0;
            var tZ = 0;
            var v = 0;

            for (var y = 0; y < gridSize; y++, v++)
            {
                for (var q = 0; q < gridSize; q++, v++)
                    tZ = SetQuadNoAlloc(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < gridSize; q++, v++)
                    tX = SetQuadNoAlloc(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < gridSize; q++, v++)
                    tZ = SetQuadNoAlloc(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < gridSize - 1; q++, v++)
                    tX = SetQuadNoAlloc(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                tX = SetQuadNoAlloc(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
            }

            tY = CreateTopFace(trianglesY, tY, ring);
            tY = CreateBottomFace(trianglesY, tY, ring);

            mesh.subMeshCount = 3;
            mesh.SetTriangles(trianglesZ, 0);
            mesh.SetTriangles(trianglesX, 1);
            mesh.SetTriangles(trianglesY, 2);
        }

        void CreateVertices(Mesh mesh)
        {
            var count = VertexCount();
            m_Colors = new Color32[count];
            m_Normals = new Vector3[count];
            m_Vertices = new Vector3[count];

            var uvs = new Vector2[m_Vertices.Length];
            var tangents = new Vector4[m_Vertices.Length];
            var tangent = new Vector4(1f, 0f, 0f, -1f);

            var v = 0;
            // side faces
            for (int y = 0; y <= gridSize; y++)
            {
                for (int x = 0; x <= gridSize; x++)
                    SetVertex(v++, x, y, 0);
                for (int z = 1; z <= gridSize; z++)
                    SetVertex(v++, gridSize, y, z);
                for (int x = gridSize - 1; x >= 0; x--)
                    SetVertex(v++, x, y, gridSize);
                for (int z = gridSize - 1; z > 0; z--)
                    SetVertex(v++, 0, y, z);
            }

            // top face
            for (int z = 1; z < gridSize; z++)
            for (int x = 1; x < gridSize; x++)
                SetVertex(v++, x, gridSize, z);

            // bottom face
            for (int z = 1; z < gridSize; z++)
            for (int x = 1; x < gridSize; x++)
                SetVertex(v++, x, 0, z);


            mesh.vertices = m_Vertices;
            mesh.colors32 = m_Colors;
            mesh.normals = m_Normals;
        }

        // Update is called once per frame
        void Generate()
        {
            var mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            mesh.name = "Procedural Sphere";

            CreateVertices(mesh);
            CreateTriangles(mesh);
            CreateColliders();
        }

        // void OnDrawGizmos()
        // {
        //     if (m_Vertices == null || m_Normals == null)
        //         return;
        //     if (m_Vertices.Length != m_Normals.Length)
        //         throw new Exception($"Mismatched size between vertices and normals array! Verts: {m_Vertices.Length} Normals: {m_Normals.Length}");
        //     Gizmos.color = Color.black;
        //     for (var i = 0; i < m_Vertices.Length; i++)
        //     {
        //         var vertex = m_Vertices[i];
        //         Gizmos.color = Color.black;
        //         Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
        //         Gizmos.color = Color.yellow;
        //         Gizmos.DrawRay(vertex, m_Normals[i]);
        //     }
        // }

        void SetVertex(int i, int x, int y, int z)
        {
            var v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
            var x2 = v.x * v.x;
            var y2 = v.y * v.y;
            var z2 = v.z * v.z;

            Vector3 s;
            s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
            m_Normals[i] = s;
            m_Vertices[i] = m_Normals[i] * radius;
            m_Colors[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
        }

        int VertexCount()
        {
            int cornerVertices = 8;
            int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
            int faceVertices = (
                    (gridSize - 1) * (gridSize - 1) +
                    (gridSize - 1) * (gridSize - 1) +
                    (gridSize - 1) * (gridSize - 1)) * 2;
            return cornerVertices + edgeVertices + faceVertices;
        }
    }
}


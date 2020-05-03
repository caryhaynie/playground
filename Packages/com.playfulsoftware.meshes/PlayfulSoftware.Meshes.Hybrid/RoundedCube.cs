using System;
using System.Linq;
using UnityEngine;

namespace PlayfulSoftware.Meshes.Hybrid
{
    using static MeshUtility;
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RoundedCube : MonoBehaviour
    {
        public int xSize;
        public int ySize;
        public int zSize;
        public int roundness;

        private Color32[] m_Colors;
        private Vector3[] m_Normals;
        private Vector3[] m_Vertices;
        // Start is called before the first frame update
        void Awake()
        {
            Generate();
        }

        int CreateBottomFace(int[] triangles, int t, int ring)
        {
            int v = 1;
            int vMid = m_Vertices.Length - (xSize - 1) * (zSize - 1);
            t = SetQuadNoAlloc(triangles, t, ring - 1, vMid, 0, 1);
            for (int x = 1; x < xSize - 1; x++, v++, vMid++) {
                t = SetQuadNoAlloc(triangles, t, vMid, vMid + 1, v, v + 1);
            }
            t = SetQuadNoAlloc(triangles, t, vMid, v + 2, v, v + 1);

            int vMin = ring - 2;
            vMid -= xSize - 2;
            int vMax = v + 2;

            for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
                t = SetQuadNoAlloc(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
                for (int x = 1; x < xSize - 1; x++, vMid++) {
                    t = SetQuadNoAlloc(
                        triangles, t,
                        vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
                }
                t = SetQuadNoAlloc(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuadNoAlloc(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (int x = 1; x < xSize - 1; x++, vTop--, vMid++) {
                t = SetQuadNoAlloc(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }
            t = SetQuadNoAlloc(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        int CreateTopFace(int[] triangles, int t, int ring)
        {
            var v = ring * ySize;
            for (var x = 0; x < xSize - 1; x++, v++)
            {
                t = SetQuadNoAlloc(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }

            t = SetQuadNoAlloc(triangles, t, v, v + 1, v + ring - 1, v + 2);

            var vMin = ring * (ySize + 1) - 1;
            var vMid = vMin + 1;
            var vMax = v + 2;

            for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuadNoAlloc(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
                for (var x = 1; x < xSize - 1; x++, vMid++)
                {
                    t = SetQuadNoAlloc(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                }
                t = SetQuadNoAlloc(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
            }

            var vTop = vMin - 2;
            t = SetQuadNoAlloc(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (var x = 1; x < xSize - 1; x++, vTop--, vMid++)
                t = SetQuadNoAlloc(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            t = SetQuadNoAlloc(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        void CreateTriangles(Mesh mesh)
        {
            var xQuads = (ySize * zSize) * 12;
            var yQuads = (xSize * zSize) * 12;
            var zQuads = (xSize * ySize) * 12;

            //var triangles = new int[quads * 6];
            var trianglesX = new int[xQuads];
            var trianglesY = new int[yQuads];
            var trianglesZ = new int[zQuads];

            var ring = (xSize + zSize) * 2;
            //var t = 0;
            var tX = 0;
            var tY = 0;
            var tZ = 0;
            var v = 0;

            for (var y = 0; y < ySize; y++, v++)
            {
                for (var q = 0; q < xSize; q++, v++)
                    tZ = SetQuadNoAlloc(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < zSize; q++, v++)
                    tX = SetQuadNoAlloc(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < xSize; q++, v++)
                    tZ = SetQuadNoAlloc(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < zSize - 1; q++, v++)
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
            for (int y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++)
                    SetVertex(v++, x, y, 0);
                for (int z = 1; z <= zSize; z++)
                    SetVertex(v++, xSize, y, z);
                for (int x = xSize - 1; x >= 0; x--)
                    SetVertex(v++, x, y, zSize);
                for (int z = zSize - 1; z > 0; z--)
                    SetVertex(v++, 0, y, z);
            }

            // top face
            for (int z = 1; z < zSize; z++)
            for (int x = 1; x < xSize; x++)
                SetVertex(v++, x, ySize, z);

            // bottom face
            for (int z = 1; z < zSize; z++)
            for (int x = 1; x < xSize; x++)
                SetVertex(v++, x, 0, z);


            mesh.vertices = m_Vertices;
            mesh.colors32 = m_Colors;
            mesh.normals = m_Normals;
        }

        // Update is called once per frame
        void Generate()
        {
            var mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            mesh.name = "Procedural Cube";

            CreateVertices(mesh);
            CreateTriangles(mesh);
            //mesh.uv = uvs;
            //mesh.tangents = tangents;
            //mesh.triangles = GenerateTriangles().SelectMany(vi => new int[] {vi.x, vi.y, vi.z}).ToArray();
            //mesh.RecalculateNormals();
        }

        void OnDrawGizmos()
        {
            if (m_Vertices == null || m_Normals == null)
                return;
            if (m_Vertices.Length != m_Normals.Length)
                throw new Exception($"Mismatched size between vertices and normals array! Verts: {m_Vertices.Length} Normals: {m_Normals.Length}");
            Gizmos.color = Color.black;
            for (var i = 0; i < m_Vertices.Length; i++)
            {
                var vertex = m_Vertices[i];
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(vertex, m_Normals[i]);
            }
        }

        void SetVertex(int i, int x, int y, int z)
        {
            var inner = m_Vertices[i] = new Vector3(x,y,z);

            SetRoundness(ref inner.x, x, xSize, roundness);
            SetRoundness(ref inner.y, y, ySize, roundness);
            SetRoundness(ref inner.z, z, zSize, roundness);

            m_Colors[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
            m_Normals[i] = (m_Vertices[i] - inner).normalized;
            m_Vertices[i] = inner + m_Normals[i] * roundness;
        }

        int VertexCount()
        {
            int cornerVertices = 8;
            int edgeVertices = (xSize + ySize + zSize - 3) * 4;
            int faceVertices = (
                    (xSize - 1) * (ySize - 1) +
                    (xSize - 1) * (zSize - 1) +
                    (ySize - 1) * (zSize - 1)) * 2;
            return cornerVertices + edgeVertices + faceVertices;
        }
    }
}


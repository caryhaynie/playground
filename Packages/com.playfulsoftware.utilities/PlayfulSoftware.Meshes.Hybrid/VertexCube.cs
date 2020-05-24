using UnityEngine;

namespace PlayfulSoftware.Meshes.Hybrid
{
    using static MeshUtility;
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class VertexCube : MonoBehaviour
    {
        public int xSize;
        public int ySize;
        public int zSize;

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
            var quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
            var triangles = new int[quads * 6];

            var ring = (xSize + zSize) * 2;
            var t = 0;
            var v = 0;

            for (var y = 0; y < ySize; y++, v++)
            {
                for (var q = 0; q < ring - 1; q++, v++)
                    t = SetQuadNoAlloc(triangles, t, v, v + 1, v + ring, v + ring + 1);
                t = SetQuadNoAlloc(triangles, t, v, v - ring + 1, v + ring, v + 1);
            }

            t = CreateTopFace(triangles, t, ring);
            t = CreateBottomFace(triangles, t, ring);
            mesh.triangles = triangles;
        }

        void CreateVertices(Mesh mesh)
        {
            m_Vertices = new Vector3[VertexCount()];
            var uvs = new Vector2[m_Vertices.Length];
            var tangents = new Vector4[m_Vertices.Length];
            var tangent = new Vector4(1f, 0f, 0f, -1f);

            var v = 0;
            // side faces
            for (int y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++)
                    m_Vertices[v++] = new Vector3(x, y, 0f);
                for (int z = 1; z <= zSize; z++)
                    m_Vertices[v++] = new Vector3(xSize, y, z);
                for (int x = xSize - 1; x >= 0; x--)
                    m_Vertices[v++] = new Vector3(x, y, zSize);
                for (int z = zSize - 1; z > 0; z--)
                    m_Vertices[v++] = new Vector3(0f, y, z);
            }

            // top face
            for (int z = 1; z < zSize; z++)
            for (int x = 1; x < xSize; x++)
                m_Vertices[v++] = new Vector3(x, ySize, z);

            // bottom face
            for (int z = 1; z < zSize; z++)
            for (int x = 1; x < xSize; x++)
                m_Vertices[v++] = new Vector3(x, 0f, z);


            mesh.vertices = m_Vertices;
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
            if (m_Vertices == null)
                return;
            Gizmos.color = Color.black;
            foreach (var vertex in m_Vertices)
            {
                Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.05f);
            }
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


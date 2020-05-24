using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayfulSoftware.Meshes.Hybrid
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class VertexGrid : MonoBehaviour
    {
        public int xSize;
        public int ySize;

        private int VertexCount => (xSize + 1) * (ySize + 1);
        private Vector3[] m_Vertices;

        // Start is called before the first frame update
        void Awake()
        {
            Generate();
        }

        // Update is called once per frame
        void Generate()
        {
            var wait = new WaitForSeconds(0.05f);

            var mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            mesh.name = "Procedural Grid";

            m_Vertices = new Vector3[VertexCount];
            var uvs = new Vector2[m_Vertices.Length];
            var tangents = new Vector4[m_Vertices.Length];
            var tangent = new Vector4(1f, 0f, 0f, -1f);
            for (int i = 0, y = 0; y <= ySize; y++)
            for (int x = 0; x <= xSize; i++, x++)
            {
                m_Vertices[i] = new Vector3(x, y);
                uvs[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;
            }

            mesh.vertices = m_Vertices;
            mesh.uv = uvs;
            mesh.tangents = tangents;
            mesh.triangles = GenerateTriangles().SelectMany(v => new int[] {v.x, v.y, v.z}).ToArray();
            mesh.RecalculateNormals();
        }

        IEnumerable<Vector3Int> GenerateTriangles()
        {
            for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                var i0 = vi;
                var i1 = vi + 1;
                var i2 = vi + xSize + 1;
                var i3 = vi + xSize + 2;
                yield return new Vector3Int(i0, i2, i1);
                yield return new Vector3Int(i1, i2, i3);
            }
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
    }
}


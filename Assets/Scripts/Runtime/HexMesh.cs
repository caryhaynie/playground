using System.Collections.Generic;
using UnityEngine;

namespace FourEx
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        Mesh m_HexMesh;
        MeshCollider m_MeshCollider;
        List<Vector3> m_Vertices;
        List<int> m_Triangles;

        void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_Vertices.Count;
            //Debug.LogFormat("adding {0} to vertex list at index {1}", v1, vertexIndex);
            m_Vertices.Add(v1);
            //Debug.LogFormat("adding {0} to vertex list at index {1}", v2, vertexIndex + 1);
            m_Vertices.Add(v2);
            //Debug.LogFormat("adding {0} to vertex list at index {1}", v3, vertexIndex + 2);
            m_Vertices.Add(v3);
            m_Triangles.Add(vertexIndex);
            m_Triangles.Add(vertexIndex + 1);
            m_Triangles.Add(vertexIndex + 2);
        }

        void Awake()
        {
            GetComponent<MeshFilter>().mesh = m_HexMesh = new Mesh();
            m_HexMesh.name = "Hex Mesh";
            m_Vertices = new List<Vector3>();
            m_Triangles = new List<int>();
            m_MeshCollider = gameObject.AddComponent<MeshCollider>();
        }

        public void Triangulate(HexCell[] cells)
        {
            m_HexMesh.Clear();
            m_Vertices.Clear();
            m_Triangles.Clear();
            for (int i = 0; i < cells.Length; i++)
            {
                Triangulate(cells[i]);
            }
            m_HexMesh.vertices = m_Vertices.ToArray();
            m_HexMesh.triangles = m_Triangles.ToArray();
            m_HexMesh.RecalculateNormals();
            m_MeshCollider.sharedMesh = m_HexMesh;
        }

        void Triangulate(HexCell cell)
        {
            var center = cell.transform.localPosition;
            for (int i = 0; i < 6; i++)
            {
                var c1 = i;
                var c2 = (i + 1) % (HexMetrics.corners.Length);
                AddTriangle(
                    center,
                    center + HexMetrics.corners[c1],
                    center + HexMetrics.corners[c2]
                );
            }
        }
    }
}
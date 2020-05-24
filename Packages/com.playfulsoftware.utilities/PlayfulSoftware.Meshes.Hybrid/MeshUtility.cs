using System.Collections.Generic;
using UnityEngine;

namespace PlayfulSoftware.Meshes.Hybrid
{
    internal static class MeshUtility
    {
        internal static void SetQuad(List<int> triangles, int v1, int v2, int v3, int v4)
        {
            triangles.Add(v1);
            triangles.Add(v3);
            triangles.Add(v2);
            triangles.Add(v2);
            triangles.Add(v3);
            triangles.Add(v4);
        }
        internal static int SetQuadNoAlloc(int[] triangles, int index, int v00, int v10, int v01, int v11)
        {
            triangles[index] = v00;
            triangles[index + 1] = triangles[index + 4] = v01;
            triangles[index + 2] = triangles[index + 3] = v10;
            triangles[index + 5] = v11;
            return index + 6;
        }

        internal static int SetQuadVerticesNoAlloc(Vector3[] vertices, int index, Vector3 v00, Vector3 v10, Vector3 v01,
            Vector3 v11)
        {
            vertices[index++] = v00;
            vertices[index++] = v10;
            vertices[index++] = v01;
            vertices[index++] = v11;
            return index;
        }

        internal static void SetRoundness(ref float value, int axis, int size, int roundness)
        {
            if (axis < roundness)
            {
                value = roundness;
            }
            else if (axis > size - roundness)
            {
                value = size - roundness;
            }
        }
    }
}
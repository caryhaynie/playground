using System;
using System.Collections.Generic;

using UnityEngine;

namespace PlayfulSoftware.Meshes.Hybrid
{
    public struct EdgeVertexList
    {
        public Vector3 firstCorner { get; set; }
        public Vector3 lastCorner { get; set; }
        public uint subdivisions { get; set; }

        private uint sectionCount => subdivisions + 1;

        public IEnumerable<Tuple<Vector3, Vector3>> pairs => vertices.Pairwise();

        public IEnumerable<Vector3> vertices
        {
            get
            {
                switch (subdivisions)
                {
                    case 0:
                    {
                        yield return firstCorner;
                        yield return lastCorner;
                        break;
                    }
                    default:
                    {
                        yield return firstCorner;
                        for (int i = 1; i < sectionCount; i++)
                        {
                            yield return Vector3.Lerp(firstCorner, lastCorner, (float)i / (float)sectionCount);
                        }
                        yield return lastCorner;
                        break;
                    }
                }
                yield break;
            }
        }
    }
}
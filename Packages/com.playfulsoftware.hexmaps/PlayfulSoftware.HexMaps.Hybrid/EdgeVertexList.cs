using System;
using System.Collections.Generic;

using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public struct EdgeVertices
    {
        public Vector3 v1, v2, v3, v4;

        public EdgeVertices(Vector3 corner1, Vector3 corner2)
        {
            v1 = corner1;
            v2 = Vector3.Lerp(corner1, corner2, 1f / 3f);
            v3 = Vector3.Lerp(corner1, corner2, 2f / 3f);
            v4 = corner2;
        }

        public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
        {
            return new EdgeVertices {
                v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step),
                v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step),
                v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step),
                v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step)
            };
        }
    }
    public struct EdgeVertexList
    {
        public Vector3 firstCorner { get; set; }
        public Vector3 lastCorner { get; set; }
        public uint subdivisions { get; set; }

        private uint sectionCount { get { return subdivisions + 1; } }

        public IEnumerable<Tuple<Vector3, Vector3>> pairs => vertices.Pairwise();

        public IEnumerable<Tuple<Vector3, Vector3>> terracedPairs => terracedVertices.Pairwise();

        public IEnumerable<Vector3> terracedVertices
        {
            get
            {
                yield return firstCorner;
                for (int i = 0; i < HexMetrics.terracedSteps; i++)
                {
                    yield return HexMetrics.TerraceLerp(firstCorner, lastCorner, i);
                }
                yield return lastCorner;
            }
        }

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

        public EdgeVertices ToEdgeVertices() => new EdgeVertices(firstCorner, lastCorner);

        public static EdgeVertexList TerraceLerp(
            EdgeVertexList a, EdgeVertexList b, int step)
        {
            return new EdgeVertexList {
                firstCorner = HexMetrics.TerraceLerp(a.firstCorner, b.firstCorner, step),
                lastCorner = HexMetrics.TerraceLerp(a.lastCorner, b.lastCorner, step)
            };
        }
    }
}
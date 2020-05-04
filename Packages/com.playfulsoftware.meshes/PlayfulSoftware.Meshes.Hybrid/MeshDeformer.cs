#if HAS_BURST_1_2_0
using Unity.Burst;
#endif // HAS_BURST_1_2_0
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace PlayfulSoftware.Meshes.Hybrid
{
    [RequireComponent(typeof(MeshFilter))]
    public sealed class MeshDeformer : MonoBehaviour
    {
        public float damping = 5f;
        public float springForce = 20f;

        private Mesh m_DeformingMesh;
        private NativeArray<Vector3> m_OriginalVertices;
        private NativeArray<Vector3> m_DisplacedVertices;
        private NativeArray<Vector3> m_VertexVelocities;

        private float m_UniformScale = 1f;

#if HAS_BURST_1_2_0
        [BurstCompile]
#endif // HAS_BURST_1_2_0
        private struct ApplyDeformingForceJob : IJobFor
        {
            [ReadOnly]
            public NativeArray<Vector3> Vertices;
            public NativeArray<Vector3> Velocities;
            public Vector3 Point;
            public float Force;
            public float DeltaTime;
            public float UniformScale;

            public void Execute(int index)
            {
                var pointToVertex = Vertices[index] - Point;
                pointToVertex *= UniformScale;
                var attenuatedForce = Force / (1f + pointToVertex.sqrMagnitude);
                var velocity = attenuatedForce * DeltaTime;
                Velocities[index] += pointToVertex.normalized * velocity;
            }
        }

#if HAS_BURST_1_2_0
        [BurstCompile]
#endif // HAS_BURST_1_2_0
        private struct UpdateVerticesJob : IJobFor
        {
            public NativeArray<Vector3> OriginalVertices;
            public NativeArray<Vector3> Vertices;
            public NativeArray<Vector3> Velocities;
            public float Damping;
            public float DeltaTime;
            public float SpringForce;
            public float UniformScale;

            public void Execute(int index)
            {
                var velocity = Velocities[index];
                var displacement = Vertices[index] - OriginalVertices[index];
                displacement *= UniformScale;
                velocity -= displacement * SpringForce * DeltaTime;
                velocity *= 1f - Damping * DeltaTime;
                Velocities[index] = velocity;
                Vertices[index] += velocity * (DeltaTime / UniformScale);
            }
        }

        void OnDestroy()
        {
            if (m_DisplacedVertices.IsCreated)
                m_DisplacedVertices.Dispose();
            if (m_OriginalVertices.IsCreated)
                m_OriginalVertices.Dispose();
            if (m_VertexVelocities.IsCreated)
                m_VertexVelocities.Dispose();
        }

        void Start()
        {
            m_DeformingMesh = GetComponent<MeshFilter>().mesh;
            m_OriginalVertices = new NativeArray<Vector3>(m_DeformingMesh.vertices, Allocator.Persistent);
            m_DisplacedVertices = new NativeArray<Vector3>(m_DeformingMesh.vertices, Allocator.Persistent);
            m_VertexVelocities = new NativeArray<Vector3>(m_OriginalVertices.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        void Update()
        {
            m_UniformScale = transform.localScale.x;
            var job = new UpdateVerticesJob
            {
                Damping = damping,
                DeltaTime = Time.deltaTime,
                SpringForce = springForce,
                UniformScale = m_UniformScale,
                OriginalVertices = m_OriginalVertices,
                Velocities = m_VertexVelocities,
                Vertices = m_DisplacedVertices
            };
            job.Run(m_DisplacedVertices.Length);
            m_DeformingMesh.SetVertices(m_DisplacedVertices);
            m_DeformingMesh.RecalculateNormals();
            m_DeformingMesh.RecalculateBounds();
        }

        public void AddDeformingForce(Vector3 point, float force)
        {
            Debug.DrawLine(Camera.main.transform.position, point, Color.black);
            var job = new ApplyDeformingForceJob
            {
                Velocities = m_VertexVelocities,
                Vertices = m_DisplacedVertices,
                DeltaTime = Time.deltaTime,
                Force = force,
                Point = transform.InverseTransformPoint(point),
                UniformScale = m_UniformScale
            };
            job.Run(m_DisplacedVertices.Length);
        }
    }
}
using UnityEngine;

namespace PlayfulSoftware.Meshes.Hybrid
{
    public sealed class MeshDeformerInput : MonoBehaviour
    {
        public float force = 10f;
        public float forceOffset = 0.1f;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
                HandleInput();
        }

        void HandleInput()
        {
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out var hit))
            {
                var deformer = hit.collider.GetComponent<MeshDeformer>();
                if (!deformer)
                    return;
                var point = hit.point;
                point += hit.normal * forceOffset;
                deformer.AddDeformingForce(point, force);
            }
        }
    }
}
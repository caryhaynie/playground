using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace PlayfulSoftware.HexMaps
{
    public sealed class HexMapCamera : MonoBehaviour
    {
        Transform m_Swivel, m_Stick;
        float m_RotationAngle;
        float m_Zoom = 1f;

        public HexGrid grid;

        [Header("Speed Controls")]
        public float moveSpeedMinZoom;
        public float moveSpeedMaxZoom;
        public float rotationSpeed;

        [Header("Stick Controls")]
        public float stickMinZoom;
        public float stickMaxZoom;
        [Header("Swivel Controls")]
        public float swivelMinZoom;
        public float swivelMaxZoom;

        void Awake()
        {
            m_Swivel = transform.GetChild(0);
            if (m_Swivel)
                m_Stick = m_Swivel.GetChild(0);
            
            AdjustZoom(m_Zoom);
        }

        void Reset()
        {
            moveSpeedMinZoom = 400f;
            moveSpeedMaxZoom = 100f;
            rotationSpeed = 100f;
            stickMinZoom = -250f;
            stickMaxZoom = -45f;
            swivelMinZoom = 90f;
            swivelMaxZoom = 45f;
        }

        void Update()
        {
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
                AdjustZoom(zoomDelta);

            float rotationDelta = Input.GetAxis("Rotation");
            if (rotationDelta != 0f)
                AdjustRotation(rotationDelta);
            
            float xDelta = Input.GetAxis("Horizontal");
            float zDelta = Input.GetAxis("Vertical");
            if (xDelta != 0f || zDelta != 0f)
                AdjustPosition(xDelta, zDelta);
        }

        void AdjustPosition(float xDelta, float zDelta)
        {
            var damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
            var dir = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            var moveSpeed = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, m_Zoom);
            var dist =  moveSpeed * damping * Time.deltaTime;
            var position = transform.localPosition;
            position += dir * dist;
            transform.localPosition = ClampPosition(position);
        }

        void AdjustRotation(float delta)
        {
            m_RotationAngle += delta * rotationSpeed * Time.deltaTime;
            m_RotationAngle = WrapAngle(m_RotationAngle, 0f, 360f);
            transform.localRotation = Quaternion.Euler(0f, m_RotationAngle, 0f);
        }

        void AdjustZoom(float delta)
        {
            m_Zoom = Mathf.Clamp01(m_Zoom + delta);

            float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, m_Zoom);
            m_Stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, m_Zoom);
            m_Swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        Vector3 ClampPosition(Vector3 position)
        {
            if (!grid)
                return position;
            var xMax = (grid.chunkCountX *
                       HexMetrics.chunkSizeX - 0.5f) *
                       (2f * HexMetrics.innerRadius);
            position.x = Mathf.Clamp(position.x, 0, xMax);
            var zMax = (grid.chunkCountZ *
                       HexMetrics.chunkSizeZ -1f) *
                       (1.5f * HexMetrics.outerRadius);
            position.z = Mathf.Clamp(position.z, 0, zMax);
            return position;
        }

        static float WrapAngle(float value, float min, float max)
        {
            if (value < min)
                return value + max;
            if (value >= max)
                return value - max;
            return value;
        }
    }
}
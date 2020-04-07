using System;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexCell : MonoBehaviour
    {
        readonly HexCell[] m_Neighbors = new HexCell[6];
        Color m_Color;
        [SerializeField] int m_Elevation = Int32.MinValue;
        bool m_HasIncomingRiver, m_HasOutgoingRiver;
        HexDirection m_IncomingRiver, m_OutgoingRiver;

        public HexCoordinates coordinates;
        [HideInInspector] public HexGridChunk chunk;
        [HideInInspector] public RectTransform uiRect;

        public int elevation
        {
            get => m_Elevation;
            set
            {
                if (m_Elevation == value)
                    return;
                m_Elevation = value;

                // verify rivers
                RemoveRiversIfInvalid();

                // Update Transform
                var pos = transform.localPosition;
                pos.y = m_Elevation * HexMetrics.elevationStep;
                pos.y +=
                    (HexMetrics.SampleNoise(pos).y * 2f - 1f) *
                    HexMetrics.elevationPerturbStrength;
                transform.localPosition = pos;

                // Update UI Transform
                var uiPosition = uiRect.localPosition;
                uiPosition.z = -pos.y;
                uiRect.localPosition = uiPosition;
                Refresh();
            }
        }

        public Vector3 position => transform.localPosition;

        public Color color
        {
            get => m_Color;
            set
            {
                if (m_Color == value)
                    return;
                m_Color = value;

                Refresh();
            }
        }

        public bool hasIncomingRiver => m_HasIncomingRiver;
        public bool hasOutgoingRiver => m_HasOutgoingRiver;
        public HexDirection incomingRiver => m_IncomingRiver;
        public HexDirection outgoingRiver => m_OutgoingRiver;

        public bool hasRiver => hasIncomingRiver || hasOutgoingRiver;
        public bool hasRiverBeginOrEnd => hasIncomingRiver != hasOutgoingRiver;

        public float riverSurfaceY => (elevation + HexMetrics.riverSurfaceElevationOffset) * HexMetrics.elevationStep;
        public float streamBedY => (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;

        public HexEdgeType GetEdgeType(HexCell otherCell)
        {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return GetEdgeType(GetNeighbor(direction));
        }

        public HexCell GetNeighbor(HexDirection direction)
        {
            return m_Neighbors[(int) direction];
        }

        public bool HasRiverThroughEdge(HexDirection dir)
        {
            return hasIncomingRiver && incomingRiver == dir ||
                   hasOutgoingRiver && outgoingRiver == dir;
        }

        public void RemoveIncomingRiver()
        {
            if (!hasIncomingRiver)
                return;
            m_HasIncomingRiver = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(incomingRiver);
            if (neighbor)
            {
                neighbor.m_HasOutgoingRiver = false;
                neighbor.RefreshSelfOnly();
            }
        }

        public void RemoveOutgoingRiver()
        {
            if (!hasOutgoingRiver)
                return;
            m_HasOutgoingRiver = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(outgoingRiver);
            if (neighbor)
            {
                neighbor.m_HasIncomingRiver = false;
                neighbor.RefreshSelfOnly();
            }
        }

        public void RemoveRiver()
        {
            RemoveIncomingRiver();
            RemoveOutgoingRiver();
        }

        void RemoveRiversIfInvalid()
        {
            if (hasOutgoingRiver)
            {
                var neighbor = GetNeighbor(outgoingRiver);
                if (neighbor && elevation < neighbor.elevation)
                    RemoveOutgoingRiver();
            }

            if (hasIncomingRiver)
            {
                var neighbor = GetNeighbor(incomingRiver);
                if (neighbor && elevation > neighbor.elevation)
                    RemoveIncomingRiver();
            }
        }

        public void SetOutgoingRiver(HexDirection dir)
        {
            // nothing to do; already have an outgoing river in this direction.
            if (hasOutgoingRiver && outgoingRiver == dir)
                return;

            var neighbor = GetNeighbor(dir);
            // rivers can't flow up-hill, so bail if neighbor elevation is higher
            // than us.
            if (!neighbor || elevation < neighbor.elevation)
                return;

            // remove the old outgoing river, if it exists.
            // additionally, if the neighbor was previously
            // an incoming river, delete that too.
            RemoveOutgoingRiver();
            if (hasIncomingRiver && incomingRiver == dir)
                RemoveIncomingRiver();

            // actually update the river state now, and refresh.
            m_HasOutgoingRiver = true;
            m_OutgoingRiver = dir;
            RefreshSelfOnly();

            // update our neighbor too.
            if (!neighbor)
                return;
            neighbor.RemoveIncomingRiver();
            neighbor.m_HasIncomingRiver = true;
            neighbor.m_IncomingRiver = dir.Opposite();
            neighbor.RefreshSelfOnly();
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            m_Neighbors[(int) direction] = cell;
            cell.m_Neighbors[(int) direction.Opposite()] = this;
        }

        void Refresh()
        {
            if (!chunk) return;
            chunk.Refresh();
            RefreshNeighborsIfInDifferentChunks();
        }

        void RefreshSelfOnly()
        {
            if (!chunk)
                return;
            chunk.Refresh();
        }

        void RefreshNeighborsIfInDifferentChunks()
        {
            for (int i = 0; i < m_Neighbors.Length; i++)
            {
                var nChunk = m_Neighbors[i]?.chunk;
                if (nChunk && nChunk != chunk)
                    nChunk.Refresh();
            }
        }

}
}
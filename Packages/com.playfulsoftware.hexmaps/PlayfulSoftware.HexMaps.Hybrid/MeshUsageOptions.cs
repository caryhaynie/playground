using System;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    [Flags]
    public enum MeshUsageOptions : byte
    {
        None = 0,
        Collider = 1 << 0,
        Colors = 1 << 1,
        [InspectorName("UV Coordinates")]
        UV = 1 << 2,
        [InspectorName("UV2 Coordinates")]
        UV2 = 1 << 3,
        [InspectorName("Terrain Type")]
        TerrainType = 1 << 4
    }

    public static class MeshUsageOptionsExtensions
    {
        public static void SetFlagValue(this MeshUsageOptions self, MeshUsageOptions flag, bool value)
        {
            if (value)
                self |= flag;
            else
                self &= ~flag;
        }
    }
}
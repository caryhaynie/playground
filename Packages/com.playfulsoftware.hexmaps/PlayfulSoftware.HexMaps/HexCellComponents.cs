using Unity.Entities;

namespace PlayfulSoftware.HexMaps
{
    // Hex Cells
    // -- Elevation
    // -- Water Level
    // -- Coordinates
    // -- Neighbors
    public struct HexCell : IComponentData
    {
        public struct Coordinates : IComponentData
        {
            public int X;
            public int Z;
        }

        public struct Features : IComponentData
        {
            public struct FarmLevel : IComponentData
            {
                public int Value;
            }
            public struct PlantLevel : IComponentData
            {
                public int Value;
            }
            public struct UrbanLevel : IComponentData
            {
                public int Value;
            }
            public byte Walled;
        }

        [InternalBufferCapacity(6)]
        public struct NeighborElement : IBufferElementData
        {
            public Entity Entity;
            public HexDirection Direction;
        }

        public struct ParentChunk : ISharedComponentData
        {
            public Entity Value;
        }

        public struct Rivers : IComponentData
        {
            public byte Incoming;
            public byte Outgoing;

            public bool HasIncoming => Incoming != 0;
            public bool HasOutgoing => Outgoing != 0;

            private HexDirection GetDirection(byte val)
            {
                if (HexDirection.NE.IsMaskSet(val))
                    return HexDirection.NE;
                if (HexDirection.E.IsMaskSet(val))
                    return HexDirection.E;
                if (HexDirection.SE.IsMaskSet(val))
                    return HexDirection.SE;
                if (HexDirection.SW.IsMaskSet(val))
                    return HexDirection.SW;
                return HexDirection.W.IsMaskSet(val) ? HexDirection.W : HexDirection.NW;
            }
        }

        public int Elevation;
        public int WaterLevel;

        public bool IsUnderWater => WaterLevel > Elevation;
    }


}
using Unity.Entities;

namespace PlayfulSoftware.HexMaps
{
    public struct HexGrid : IComponentData
    {
        public struct ChunkInfo : ISystemStateComponentData
        {
            public int ChunkCountX;
            public int ChunkCountZ;
        }

        public struct CellElement : ISystemStateBufferElementData
        {
            public Entity Value;
        }

        public struct ChunkElement : ISystemStateBufferElementData
        {
            public Entity Value;
        }

        public int CellCountX;
        public int CellCountZ;
    }
}
namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Data
{
    public struct ChunkChange
    {
        public ChunkChangeType ChangeType;
        public ulong MortonCode;
        public ChunkPayload? Payload;
    }

    public enum ChunkChangeType
    {
        Load,
        Unload,
        Update
    }
}
namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    public struct ChunkChange
    {
        public ChunkCoord Coord;
        public ChunkChangeType Type;

        public ChunkChange(ChunkCoord coord, ChunkChangeType type)
        {
            Coord = coord;
            Type = type;
        }
    }

    public enum ChunkChangeType
    {
        Load,
        Unload,
        Update
    }
}
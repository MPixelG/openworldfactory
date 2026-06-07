namespace _Project.World.Planet.Scripts.Chunking.Core
{
    
    /// <summary>
    /// a chunk change is a change the chunk manager made. this is used to signal the unity side of the project that doesnt have direct access to the data what changes have been made so that the graphics get updated correctly. 
    /// </summary>
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

    /// <summary>
    /// the type of the ChunkChange. 
    /// </summary>
    public enum ChunkChangeType
    {
        Load,
        Unload,
        Update
    }
}
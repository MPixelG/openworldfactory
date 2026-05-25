using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking
{
    /// <summary>
    /// the base for all different chunk systems. so both a tessellated chunk system and a grid based chunk system have to contain the methods GetChunkAt and SetChunkAt. 
    /// </summary>
    public abstract class ChunkSystem
    {

        public abstract Chunk GetChunkAt(int3 position);
        public abstract void SetChunkAt(int3 position, Chunk chunk);
        
        
        


    }
}
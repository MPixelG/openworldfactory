using System.Collections.Generic;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking
{
    public class GridChunkSystem : ChunkSystem
    {
        
        Dictionary<int3,  Chunk> chunks = new Dictionary<int3, Chunk>();
        
        public override Chunk GetChunkAt(int3 position)
        {
            return chunks[position];
        }

        public override void SetChunkAt(int3 position, Chunk chunk)
        {
            chunks[position] = chunk;
        }
    }
}
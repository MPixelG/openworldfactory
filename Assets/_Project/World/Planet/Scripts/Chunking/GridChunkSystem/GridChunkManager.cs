using System.Collections.Generic;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    public class GridChunkManager
    {
        
        private readonly Dictionary<int3, Chunk> _chunks = new();
        
        public Chunk GetChunkAt(int3 position) => _chunks[position];
        public void SetChunkAt(int3 position, Chunk chunk) => _chunks[position] = chunk;
    }
}
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Collections;

namespace _Project.World.Planet.Scripts.v2
{
    public class ChunkDataStore
    {
        public NativeHashMap<ulong, ChunkData> Chunks = new();
        public NativeHashMap<ulong, ChunkPayload> ChunkPayloads = new();
        
        
        public void Dispose()
        {
            Chunks.Dispose();
            ChunkPayloads.Dispose();
        }
    }
}
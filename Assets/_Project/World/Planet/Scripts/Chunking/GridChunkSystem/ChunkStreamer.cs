using System.Collections.Generic;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    public class ChunkStreamer
    {
        private readonly int _chunkSize;

        public ChunkStreamer(int chunkSize)
        {
            _chunkSize = chunkSize;
        }

        private ChunkCoord _lastCalculatedChunkCoord;
        public HashSet<ChunkCoord> ComputeVisibleChunks(
            float3 viewerPosition,
            int viewDistanceInChunks
        )
        {
            ChunkCoord viewerChunk = new(
                (int3) math.floor(viewerPosition / _chunkSize)
            );

            if (_lastCalculatedChunkCoord.Equals(viewerChunk) || viewDistanceInChunks == 0) return new HashSet<ChunkCoord>();
            
            // calculate a sphere of chunks that need to be generated
            HashSet<ChunkCoord> visibleChunks = new();
            for (int x = -viewDistanceInChunks; x <= viewDistanceInChunks; x++)
            {
                for (int y = -viewDistanceInChunks; y <= viewDistanceInChunks; y++)
                {
                    for (int z = -viewDistanceInChunks; z <= viewDistanceInChunks; z++)
                    {
                        ChunkCoord chunkCoord = viewerChunk + new int3(x, y, z);
                        if (math.distance(viewerChunk.Value, chunkCoord.Value) <= viewDistanceInChunks)
                        {
                            visibleChunks.Add(chunkCoord);
                        }
                    }
                }
            }
            
            
            return visibleChunks;
        }
    }
}
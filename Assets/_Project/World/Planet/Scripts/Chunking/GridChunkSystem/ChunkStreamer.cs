using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.Core;
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
        private bool _hasLastCalculated;
        private HashSet<ChunkCoord> _lastVisibleChunks = new();

        public HashSet<ChunkCoord> ComputeVisibleChunks(
            float3 viewerPosition,
            int viewDistanceInChunks
        )
        {
            ChunkCoord viewerChunk = new(
                (int3) math.floor(viewerPosition / _chunkSize)
            );

            if (viewDistanceInChunks <= 0)
            {
                _lastCalculatedChunkCoord = viewerChunk;
                _hasLastCalculated = true;
                _lastVisibleChunks.Clear();
                return new HashSet<ChunkCoord>();
            }

            if (_hasLastCalculated && _lastCalculatedChunkCoord.Equals(viewerChunk))
            {
                return new HashSet<ChunkCoord>(_lastVisibleChunks);
            }

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

            _lastCalculatedChunkCoord = viewerChunk;
            _hasLastCalculated = true;
            _lastVisibleChunks = visibleChunks;

            return new HashSet<ChunkCoord>(_lastVisibleChunks);
        }
    }
}
using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.Core;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    /// <summary>
    /// the chunk streamer is responsible for calculating what chunks to display.
    /// it has no reference to any chunk systems and only contains a function that returns a set of chunks that should be visible based on the viewers position (in world pos).
    /// everything else is managed by the chunk manager.
    /// </summary>
    public class ChunkStreamer
    {
        private readonly int _chunkSize;

        public ChunkStreamer(int chunkSize)
        {
            _chunkSize = chunkSize;
        }

        private ChunkCoord _lastCalculatedChunkCoord; // the chunk coordinate of the last viewer position where the visible chunk coords were calculated for. this is used to avoid recalculating the visible chunks if the viewer is still in the same chunk as the last calculation
        private bool _hasLastCalculated; // indicates whether there has been a calculation of visible chunks yet 
        private HashSet<ChunkCoord> _lastVisibleChunks = new(); // contains the visible chunks of the last valid calculation

        /// <summary>
        /// calculates the visible chunks based on the viewers position and the view distance in chunks
        /// </summary>
        /// <param name="viewerPosition"> the world position of the viewer</param>
        /// <param name="viewDistanceInChunks"> the viewers view distance in chunks. it is used as a radius of a sphere around the viewer where every chunk thats inside that sphere is returned. </param>
        /// <returns> a set of chunk coords of visible chunks</returns>
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
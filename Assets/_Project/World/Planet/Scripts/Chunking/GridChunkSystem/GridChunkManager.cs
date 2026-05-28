using System;
using System.Collections.Generic;
using System.Linq;
using _Project.World.Planet.Scripts.WorldGen.Samplers;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    public class GridChunkManager
    {
        private readonly Dictionary<ChunkCoord, ChunkData> _chunks = new();
        private readonly ChunkGenerator _chunkGenerator;
        private readonly ChunkStreamer _chunkStreamer;
        private readonly int _chunkSize;

        public event Action<ChunkChange> ChunkChange;
        
        public ChunkData GetChunkAt(ChunkCoord position) => _chunks[position];
        public IEnumerable<ChunkCoord> GetLoadedChunkCoords() => _chunks.Keys;

        private void SetChunkAt(ChunkCoord position, ChunkData chunkData)
        {
            _chunks[position] = chunkData;
            ChunkChange?.Invoke(new ChunkChange(position, ChunkChangeType.Update));
        }


        public GridChunkManager(int chunkSize)
        {
            _chunkSize = chunkSize;
            
            _chunkGenerator = new ChunkGenerator(
                densitySampler: new SphereSampler(),
                chunkSize: _chunkSize
                );
            _chunkStreamer = new ChunkStreamer(_chunkSize);
        }


        public void Update(float3 viewerPosition)
        {
            SyncChunks(viewerPosition);
        }

        private void SyncChunks(float3 viewerPosition)
        {
            HashSet<ChunkCoord> chunksToKeep = _chunkStreamer.ComputeVisibleChunks(
                viewerPosition: viewerPosition,
                viewDistanceInChunks: 4
            );

            IEnumerable<ChunkCoord> filteredChunksToKeep = chunksToKeep.Where(chunk => !_chunks.ContainsKey(chunk));
            
            foreach (var chunk in filteredChunksToKeep)
            {
                LoadChunkAt(chunk);
            }
            
            
            HashSet<ChunkCoord> chunksToUnload = new(_chunks.Keys);
            if (chunksToUnload == null) throw new ArgumentNullException(nameof(chunksToUnload));
            chunksToUnload.ExceptWith(chunksToKeep);
            
            foreach (var chunk in chunksToUnload)
            {
                UnloadChunkAt(chunk);
            }
            
        }


        private void LoadChunkAt(ChunkCoord position)
        {
            ChunkData chunkData = _chunkGenerator.GenerateChunkAt(position);
            _chunks[position] = chunkData;
            ChunkChange?.Invoke(new ChunkChange(position, ChunkChangeType.Load));
        }

        private void UnloadChunkAt(ChunkCoord position)
        {
            _chunks.Remove(position);
            ChunkChange?.Invoke(new ChunkChange(position, ChunkChangeType.Unload));
        }
    }
}
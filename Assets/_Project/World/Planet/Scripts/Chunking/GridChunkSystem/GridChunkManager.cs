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
        private ChunkGenerator _chunkGenerator;
        private ChunkStreamer _chunkStreamer;
        private readonly int _chunkSize;
        
        public ChunkData GetChunkAt(ChunkCoord position) => _chunks[position];
        public void SetChunkAt(ChunkCoord position, ChunkData chunkData) => _chunks[position] = chunkData;


        public GridChunkManager(int chunkSize)
        {
            _chunkSize = chunkSize;
            
            _chunkGenerator = new ChunkGenerator(
                densitySampler: new SphereSampler(),
                chunkSize: chunkSize
                );
        }


        public void Update()
        {

            
            
            


        }

        private void SyncChunks()
        {
            HashSet<ChunkCoord> chunksToKeep = _chunkStreamer.ComputeVisibleChunks(
                viewerPosition: int3.zero,
                viewDistanceInChunks: 4
            );
            
            IEnumerable<ChunkCoord> filteredChunksToKeep = chunksToKeep.Where(chunk => !_chunks.ContainsKey(chunk) || _chunks[chunk] == null);
            
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
        }

        private void UnloadChunkAt(ChunkCoord position)
        {
            _chunks[position] = null;
        }
    }
}
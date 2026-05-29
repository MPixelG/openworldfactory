using System;
using System.Collections.Generic;
using System.Linq;
using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    public class GridChunkManager
    {
        private readonly Dictionary<ChunkCoord, ChunkData> _chunks = new();
        private readonly ChunkGenerator _chunkGenerator;
        private readonly ChunkStreamer _chunkStreamer;
        public readonly int ChunkSize;
        private readonly int _viewDistanceInChunks;

        public event Action<ChunkChange> ChunkChange;

        private readonly Dictionary<ChunkCoord, ChunkLifecycleState> _chunkStates = new();


        private readonly Queue<ChunkCoord> _loadQueue = new();
        private readonly Queue<ChunkCoord> _unloadQueue = new();

        public ChunkData GetChunkAt(ChunkCoord position) => _chunks[position];
        public IEnumerable<ChunkCoord> GetLoadedChunkCoords() => _chunks.Keys;

        private void SetChunkAt(ChunkCoord position, ChunkData chunkData)
        {
            _chunks[position] = chunkData;
            ChunkChange?.Invoke(new ChunkChange(position, ChunkChangeType.Update));
        }


        public GridChunkManager(int chunkSize, int viewDistanceInChunks, IDensitySampler densitySampler)
        {
            ChunkSize = chunkSize;
            _viewDistanceInChunks = viewDistanceInChunks;

            _chunkGenerator = new ChunkGenerator(
                densitySampler: densitySampler,
                chunkSize: ChunkSize
            );
            _chunkStreamer = new ChunkStreamer(ChunkSize);
        }


        public void Update(float3 viewerPosition)
        {
            
            const int chunksPerFrame = 1;

            for (int i = 0; i < chunksPerFrame; i++)
            {
                if(_unloadQueue.Count > 0)
                    UnloadChunkAt(_unloadQueue.Dequeue());
                else if (_loadQueue.Count > 0)
                    LoadChunkAt(_loadQueue.Dequeue());

                else break;
            }
            
            SyncChunks(viewerPosition);
        }

        private void SyncChunks(float3 viewerPosition)
        {
            HashSet<ChunkCoord> chunksToKeep = _chunkStreamer.ComputeVisibleChunks(
                viewerPosition: viewerPosition,
                viewDistanceInChunks: _viewDistanceInChunks
            );

            IEnumerable<ChunkCoord> filteredChunksToKeep = chunksToKeep.Where(chunk => !_chunks.ContainsKey(chunk));

            foreach (var chunk in filteredChunksToKeep)
            {
                if (_chunkStates.TryGetValue(chunk, out var state))
                {
                    if (state != ChunkLifecycleState.Unloaded)
                        continue;
                }

                _chunkStates[chunk] = ChunkLifecycleState.Queued;
                _loadQueue.Enqueue(chunk);
            }


            HashSet<ChunkCoord> chunksToUnload = new(_chunks.Keys);
            chunksToUnload.ExceptWith(chunksToKeep);

            foreach (var chunk in chunksToUnload)
            {
                if (_chunkStates.TryGetValue(chunk, out var state))
                {
                    if (state != ChunkLifecycleState.Loaded)
                        continue;
                }

                _chunkStates[chunk] = ChunkLifecycleState.Queued;
                _unloadQueue.Enqueue(chunk);
            }
        }


        private void LoadChunkAt(ChunkCoord position)
        {
            _chunkStates[position] = ChunkLifecycleState.Generating;
            ChunkData chunkData = _chunkGenerator.GenerateChunkAt(position);
            _chunks[position] = chunkData;
            ChunkChange?.Invoke(new ChunkChange(position, ChunkChangeType.Load));
            _chunkStates[position] = ChunkLifecycleState.Loaded;
        }

        private void UnloadChunkAt(ChunkCoord position)
        {
            _chunks.Remove(position);
            ChunkChange?.Invoke(new ChunkChange(position, ChunkChangeType.Unload));
            _chunkStates.Remove(position);
        }
    }
}

enum ChunkLifecycleState
{
    Unloaded,
    Queued,
    Generating,
    Loaded
}
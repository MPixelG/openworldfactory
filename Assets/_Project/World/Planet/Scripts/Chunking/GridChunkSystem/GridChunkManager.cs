using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    public class GridChunkManager
    {
        private readonly Dictionary<ChunkCoord, ChunkData> _chunks = new();
        private readonly Dictionary<ChunkCoord, ChunkLifecycleState> _chunkStates = new();

        private readonly Queue<ChunkCoord> _loadQueue = new();
        private readonly Queue<ChunkCoord> _unloadQueue = new();

        private readonly ConcurrentQueue<(ChunkCoord coord, ChunkData data)>
            _completedChunks = new();

        private readonly ChunkGenerator _chunkGenerator;
        private readonly ChunkStreamer _chunkStreamer;

        private int3 _lastViewerChunk;
        private bool _hasLastViewerChunk;

        private int _activeGenerationTasks;

        private const int ChunksPerFrame = 1;
        private const int MaxConcurrentGenerationTasks = 8;

        public readonly int ChunkSize;
        private readonly int _viewDistanceInChunks;

        public event Action<ChunkChange> ChunkChange;

        public GridChunkManager(
            int chunkSize,
            int viewDistanceInChunks,
            IDensitySampler densitySampler
        )
        {
            ChunkSize = chunkSize;
            _viewDistanceInChunks = viewDistanceInChunks;

            _chunkGenerator = new ChunkGenerator(
                densitySampler,
                chunkSize
            );

            _chunkStreamer = new ChunkStreamer(chunkSize);
        }

        public ChunkData GetChunkAt(ChunkCoord coord)
        {
            return _chunks[coord];
        }

        public IEnumerable<ChunkCoord> GetLoadedChunkCoords()
        {
            return _chunks.Keys;
        }

        public void Update(float3 viewerPosition)
        {
            ProcessCompletedChunks();

            SyncChunks(viewerPosition);

            ProcessQueues();
        }

        private void ProcessQueues()
        {
            for (int i = 0; i < ChunksPerFrame; i++)
            {
                if (_unloadQueue.Count <= 0)
                    break;

                ChunkCoord coord = _unloadQueue.Dequeue();

                UnloadChunk(coord);
            }

            for (int i = 0; i < ChunksPerFrame; i++)
            {
                if (_activeGenerationTasks >= MaxConcurrentGenerationTasks)
                    break;

                if (_loadQueue.Count <= 0)
                    break;

                ChunkCoord coord = _loadQueue.Dequeue();

                _ = GenerateChunkAsync(coord);
            }
        }

        private void SyncChunks(float3 viewerPosition)
        {
            int3 viewerChunk =
                (int3)math.floor(viewerPosition / ChunkSize);

            if (_hasLastViewerChunk &&
                viewerChunk.Equals(_lastViewerChunk))
            {
                return;
            }

            _lastViewerChunk = viewerChunk;
            _hasLastViewerChunk = true;

            HashSet<ChunkCoord> visibleChunks =
                _chunkStreamer.ComputeVisibleChunks(
                    viewerPosition,
                    _viewDistanceInChunks
                );

            foreach (ChunkCoord coord in visibleChunks)
            {
                if (_chunkStates.TryGetValue(coord, out var state))
                {
                    if (state != ChunkLifecycleState.Unloaded)
                        continue;
                }

                _chunkStates[coord] = ChunkLifecycleState.Queued;

                _loadQueue.Enqueue(coord);
            }

            HashSet<ChunkCoord> chunksToUnload =
                new(_chunks.Keys);

            chunksToUnload.ExceptWith(visibleChunks);

            foreach (ChunkCoord coord in chunksToUnload)
            {
                if (!_chunkStates.TryGetValue(coord, out var state))
                    continue;

                if (state != ChunkLifecycleState.Loaded)
                    continue;

                _chunkStates[coord] = ChunkLifecycleState.QueuedForUnload;

                _unloadQueue.Enqueue(coord);
            }
        }

        private async Task GenerateChunkAsync(ChunkCoord coord)
        {
            _activeGenerationTasks++;

            _chunkStates[coord] = ChunkLifecycleState.Generating;

            try
            {
                ChunkData chunkData = await Task.Run(() =>
                {
                    return _chunkGenerator.GenerateChunkAt(coord);
                });

                _completedChunks.Enqueue((coord, chunkData));
            }
            finally
            {
                _activeGenerationTasks--;
            }
        }

        private void ProcessCompletedChunks()
        {
            while (_completedChunks.TryDequeue(out var result))
            {
                ChunkCoord coord = result.coord;
                ChunkData data = result.data;

                if (!_chunkStates.TryGetValue(coord, out var state))
                    continue;

                if (state != ChunkLifecycleState.Generating)
                    continue;

                _chunks[coord] = data;

                _chunkStates[coord] = ChunkLifecycleState.Loaded;

                ChunkChange?.Invoke(
                    new ChunkChange(coord, ChunkChangeType.Load)
                );
            }
        }

        private void UnloadChunk(ChunkCoord coord)
        {
            _chunks.Remove(coord);

            _chunkStates.Remove(coord);

            ChunkChange?.Invoke(
                new ChunkChange(coord, ChunkChangeType.Unload)
            );
        }
    }

    public enum ChunkLifecycleState
    {
        Unloaded,
        Queued,
        Generating,
        Loaded,
        QueuedForUnload
    }
}
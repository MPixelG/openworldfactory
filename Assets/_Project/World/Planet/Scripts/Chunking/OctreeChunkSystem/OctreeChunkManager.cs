using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem
{
    public class OctreeChunkManager
    {
        private readonly Dictionary<NodeKey, OctreeChunkData> _chunks = new();
        private readonly Dictionary<NodeKey, OctreeChunkLifecycleState> _chunkStates = new(); //todo use ChunkState instead

        public Octree Octree;

        public int3 Origin;
        public float Size;
        
        private BurstSamplerSettings _densitySamplerSettings;

        private readonly Queue<NodeKey>
            _loadQueue =
                new(); // queues for chunks that need to be loaded / unloaded. those get emptied once the tasks are done

        private readonly Queue<NodeKey> _unloadQueue = new();

        private readonly ConcurrentQueue<(NodeKey coord, OctreeChunkData data)>
            _completedChunks =
                new(); // another queue that gets filled once the data is generated. those values will then get shifted over to the chunks and chunk state dictionaries.

        private readonly ChunkGenerator
            _chunkGenerator; // the generator that generates the density values of the chunks
        
        private int3
            _lastViewerChunk; // the last chunk the viewer was inside. its used to skip unnecessary calculations that were calculated exactly like that in the previous frame already

        private bool _hasLastViewerChunk; // if the last viewer chunk is not yet set

        private int _activeGenerationTasks; // number of current chunk density generation tasks

        private const int ChunksPerFrame = 20; // maximum amount of chunks that can get generated per frame
        private const int MaxConcurrentGenerationTasks = 2; // max concurrent (simultaneous) chunk generations
        
        
        private readonly int
            _viewDistanceInChunks; // the view distance of the viewer (the radius of the "sphere" of chunk that are kept around the viewer) 

        public event Action<NodeChange>
            NodeChange; // an action that gets called once a chunk was changed. other classes like the renderer can listen to this and react to changes (like loads, unloads or updates) to update their values.

        /// <param name="origin">the origin world pos of the system</param>
        /// <param name="size">the size of the system in world space</param>
        /// <param name="densitySamplerSettings">the settings used for generating the density values</param>
        public OctreeChunkManager(
            Vector3Int origin,
            float size,
            BurstSamplerSettings densitySamplerSettings
        )
        {
            Origin = new int3(origin.x, origin.y, origin.z);
            Size = size;

            /*_chunkGenerator = new ChunkGenerator(
                densitySamplerSettings,
                chunkSize
            );*/

            _densitySamplerSettings = densitySamplerSettings;

            RebuildOctree();
        }

        public void RebuildOctree()
        {
            Octree = OctreeHelper.Build(Origin, new int3((int) Size, (int) Size, (int) Size), _densitySamplerSettings, 6);
        }

        /// <summary>
        /// returns the chunk data of a given position. if no chunk exists at that position, null is returned.
        /// </summary>
        /// <param name="coord">the coordinate of the chunk</param>
        /// <returns>the chunk data of the chunk at that position. may be null.</returns>
        public OctreeChunkData GetChunkAt(NodeKey coord)
        {
            return _chunks.GetValueOrDefault(coord, null);
        }

        /// <summary>
        /// returns the coordinates of the currently loaded chunks
        /// </summary>
        /// <returns>the chunk coordinates of the currently loaded chunks</returns>
        public IEnumerable<NodeKey> GetLoadedChunkCoords()
        {
            return _chunks.Keys;
        }

        /// <summary>
        /// updates the chunk manager. this should be called once per frame.
        /// it processes the completed chunk generations, syncs the loaded chunks with the viewers position and processes the load / unload queues.
        /// this should only be called by the renderer as it is inefficient to update the manager multiple times per frame as it only changes across frames and not within a single frame. 
        /// </summary>
        /// <param name="viewerPosition">the position of the viewer as a world position</param>
        public void Update(float3 viewerPosition)
        {
            ProcessCompletedChunks();

            SyncChunks(viewerPosition);

            ProcessQueues();
        }

        /// <summary>
        /// loads / unloads chunks if they are in the different queueues. it doesnt check what chunks need to be loaded / unloaded!
        /// </summary>
        private void ProcessQueues()
        {
            int chunkCounter = 0; // counts the chunks we loaded or unloaded in this frame

            while
                (chunkCounter <
                 ChunksPerFrame) // we only want to load / unload a certain amount of chunks per frame to avoid performance spikes
            {
                if (_unloadQueue.Count <= 0) // break the loop if there are no more chunks to unload  
                    break;

                NodeKey coord = _unloadQueue.Dequeue(); // retrieve a chunk coord to unload from the queue

                UnloadChunk(coord);
                chunkCounter++;
            }

            while
                (chunkCounter <
                 ChunksPerFrame) // we reuse the chunk counter without resetting it to ensure we only do a limited amount of operations (not loads and unloads) per frame
            {
                if (_activeGenerationTasks >=
                    MaxConcurrentGenerationTasks) // break if we cant start any more tasks this frame
                    break;

                if (_loadQueue.Count <= 0) // or break if the queue is empty
                    break;

                NodeKey coord = _loadQueue.Dequeue();

                _ = GenerateChunkAsync(
                    coord); // we dont need the result of the task as its already automatically getting stored, but we need to add a reference to the task for it to start so we just use a tmp variable

                chunkCounter++;
            }
        }

        /// <summary>
        /// syncs the loaded chunks with the viewers position.
        /// it calculates what chunks should be loaded based on the viewers position and the view distance and adds those to the load queue if they are not already loaded.
        /// it also calculates what chunks should be unloaded and adds those to the unload queue if they are currently loaded.
        /// it does not actually loads or unloads the chunks, the processQueues method is responsible for that.
        /// </summary>
        /// <param name="viewerPosition"></param>
        private void SyncChunks(float3 viewerPosition)
        {
            //TODO
            /*
            int3 viewerChunk =
                (int3)math.floor(viewerPosition / ChunkSize); // convert world to chunk space

            if (_hasLastViewerChunk && viewerChunk.Equals(_lastViewerChunk)) // if nothing changed since the last frames the results will be the same so we can exit
            {
                return;
            }

            _lastViewerChunk = viewerChunk;
            _hasLastViewerChunk = true;

            foreach (NodeKey coord in visibleChunks)
            {
                if (_chunkStates.TryGetValue(coord, out var state)) // try to get the current state of that chunk
                {
                    if (state != OctreeChunkLifecycleState.Unloaded) // if the chunk is anything else than unloaded (like loaded, generating etc.) we dont need to do anything 
                        continue;
                }

                _chunkStates[coord] = OctreeChunkLifecycleState.Queued; // set the state to queued

                _loadQueue.Enqueue(coord); // append it to the load queue
            }

            HashSet<NodeKey> chunksToUnload = new(_chunks.Keys); 
            
            chunksToUnload.ExceptWith(visibleChunks); // every chunk thats currently loaded but doesnt need to be loaded gets unloaded

            foreach (NodeKey coord in chunksToUnload)
            {
                if (!_chunkStates.TryGetValue(coord, out var state)) // if its state doesnt exist anymore it indicates that it probably got unloaded already 
                    continue;

                if (state != OctreeChunkLifecycleState.Loaded) // if its currently unloaded, building, queued, etc. we ignore it
                    continue;

                _chunkStates[coord] = OctreeChunkLifecycleState.QueuedForUnload;

                _unloadQueue.Enqueue(coord);
            } */
        }

        /// <summary>
        /// generates a chunk at a given position using the chunk generator
        /// </summary>
        /// <param name="coord">the coordinate of the chunk that gets generated</param>
        private async Task GenerateChunkAsync(NodeKey coord)
        {
            Interlocked.Increment(ref _activeGenerationTasks); // keep track of the current amount of chunks that are being generated since they can generate across multiple frames

            _chunkStates[coord] = OctreeChunkLifecycleState.Generating; // set the current state to generating

            try
            {
                //OctreeChunkData chunkData = await Task.Run(() => ); // run the generation job
                //TODO
                //_completedChunks.Enqueue((coord, chunkData)); // and add its data afterward.
                                                              // this _completedChunks queue will get processed in the main thread in the update method to shift the
                                                              // generated data over to the main dictionaries and trigger the chunk change event
            }
            catch (Exception e)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogException(e);
            }
            finally
            {
                Interlocked.Decrement(ref _activeGenerationTasks); // and finally (even if the task failed) remove it from the active tasks count
            }
        }

        /// <summary>
        /// processes the completed chunk generations.
        /// it shifts the generated data from the _completedChunks queue to the main _chunks and _chunkStates dictionaries and triggers the chunk change event for the loaded chunks.
        /// this should only be called in the main thread (like in the update method) as it accesses the main dictionaries that are not thread safe.
        /// </summary>
        private void ProcessCompletedChunks()
        {
            while (_completedChunks.TryDequeue(out var result)) // every completed chunk gets processed
            {
                NodeKey coord = result.coord;
                OctreeChunkData data = result.data;

                if (!_chunkStates.TryGetValue(coord, out var state)) // if the state of the chunk doesnt exist, its broken as the state is already generated before starting the generation task. so to prevent further errors we just ignore that case
                    continue;

                if (state != OctreeChunkLifecycleState.Generating) // the state should currently be in the "generating" phase
                    continue;

                _chunks[coord] = data; // apply the data to the main chunks dictionary

                _chunkStates[coord] = OctreeChunkLifecycleState.Loaded; // and update the state to loaded

                NodeChange?.Invoke( // report the change to the listeners
                    new NodeChange(coord, Unity.NodeChange.NodeChangeType.Load)
                );
            }
        }

        /// <summary>
        /// unloads the chunk at a given position
        /// </summary>
        /// <param name="coord">the coordinate of the chunk to unload</param>
        private void UnloadChunk(NodeKey coord)
        {
            //_chunks[coord].DensityField.Dispose(); // dispose the density field of that chunk to free up memory
            
            _chunks.Remove(coord); // remove its data

            _chunkStates.Remove(coord); // and state

            NodeChange?.Invoke(
                new NodeChange(coord, Unity.NodeChange.NodeChangeType.Unload) // and report the change to the listeners
            );
        }
    }
    
    public enum OctreeChunkLifecycleState
    {
        Unloaded,
        Queued,
        Generating,
        Loaded,
        QueuedForUnload
    }
}
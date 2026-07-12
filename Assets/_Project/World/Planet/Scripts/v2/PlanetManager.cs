using System;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.v2.Data;
using _Project.World.Planet.Scripts.v2.Unity;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2
{
    public class PlanetManager
    {
        private PlanetConfig _config;


        public Octree Octree { get; private set; }
        private readonly ChunkDataStore _chunkDataStore;
        
        private readonly ChunkGenerationPipeline _chunkGenerationPipeline;

        public event Action<ChunkChange> ChunkChange; 
        
        public PlanetManager(PlanetConfig config)
        {
            _config = config;
            _chunkDataStore = new ChunkDataStore();
            _chunkGenerationPipeline = new ChunkGenerationPipeline();
            _chunkGenerationPipeline.OnChunkGenerated += OnChunkGenerated;
        }

        private OctreeBuilder _octreeBuilder;
        public bool OctreeReady;
        public void RebuildOctree()
        {
            _octreeBuilder = new OctreeBuilder(_config.origin, 10, _config.samplerSettings);
            _octreeBuilder.Build();

            OctreeReady = false;
            
            _chunkGenerationPipeline.UpdateMin(_config.origin);
            _chunkGenerationPipeline.UpdateMaxDepth(Octree.MaxDepth);
            _chunkGenerationPipeline.UpdateSamplerSettings(_config.samplerSettings);
            _chunkGenerationPipeline.UpdateChunkSize(_config.chunkSize);
            ClearChunks();
        }

        public void Update()
        {
            _chunkGenerationPipeline.Update();
            _octreeBuilder.Update();
            if (_octreeBuilder.IsDone() && !OctreeReady)
            {
                Octree? tree = _octreeBuilder.GetReadyTree();
                Debug.Assert(tree != null, "Octree is null!");
                Octree = tree.Value;
                OctreeReady = true;
                Debug.Log("OCTREE READY, node count: " + Octree.Nodes.Length);
                foreach (OctreeNode octreeNode in Octree.Nodes)
                {
                    if (octreeNode.State != OctreeNodeState.Mixed) continue;
                    if (octreeNode.MortonCode.GetDepth() != 3) continue;
                    _chunkGenerationPipeline.QueueGenerationAt(octreeNode.MortonCode);
                }
                
            }
        }
        
        
        public void UpdateConfig(PlanetConfig config)
        {
            _config = config;
            _chunkGenerationPipeline.UpdateMin(config.origin);
            _chunkGenerationPipeline.UpdateSamplerSettings(config.samplerSettings);
            _chunkGenerationPipeline.UpdateChunkSize(config.chunkSize);
        }
        
        private void OnChunkGenerated(ChunkGeneration chunkGeneration)
        {
            _chunkDataStore.SetChunkPayloadAt(chunkGeneration.MortonCode, chunkGeneration.Payload);
            
            ChunkChange change = new ChunkChange
            {
                ChangeType = ChunkChangeType.Load,
                MortonCode = chunkGeneration.MortonCode,
                Payload = chunkGeneration.Payload
            };
            ChunkChange?.Invoke(change);
        }
        
        private void ClearChunks()
        {
            foreach (ulong mortonCode in _chunkDataStore.GetMortonCodes())
            {
                ChunkChange change = new ChunkChange
                {
                    ChangeType = ChunkChangeType.Unload,
                    MortonCode = mortonCode,
                    Payload = null
                };
                ChunkChange?.Invoke(change);
            }
            _chunkDataStore.Clear();
        }

        public void Dispose()
        {
            Octree.Dispose();
            _chunkGenerationPipeline.OnChunkGenerated -= OnChunkGenerated;
        }
    }
}
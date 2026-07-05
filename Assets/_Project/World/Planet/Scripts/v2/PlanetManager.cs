using System;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.v2.Data;
using _Project.World.Planet.Scripts.v2.Unity;
using Unity.Mathematics;
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
        
        public bool OctreeReady {get; private set; }
        public void RebuildOctree()
        {
            Octree = OctreeHelper.Build(_config.origin, _config.origin + new int3(_config.size), _config.samplerSettings, 1);
            Debug.Log("Octree Built");
            OctreeReady = true;
            
            _chunkGenerationPipeline.UpdateMin(_config.origin);
            _chunkGenerationPipeline.UpdateMaxDepth(Octree.MaxDepth);
            _chunkGenerationPipeline.UpdateSamplerSettings(_config.samplerSettings);
            _chunkGenerationPipeline.UpdateChunkSize(_config.chunkSize);
            Debug.Log("Everything updated");
            ClearChunks();
            Debug.Log("Chunks cleared");
            
            foreach (OctreeNode octreeNode in Octree.Nodes)
            {
                if (octreeNode.State != OctreeNodeState.Mixed) continue;
                if (octreeNode.MortonCode.GetDepth() != 4) continue;
                Debug.Log("Queued!");
                _chunkGenerationPipeline.QueueGenerationAt(octreeNode.MortonCode);
            }
        }

        public void Update()
        {
            _chunkGenerationPipeline.Update();
        }
        
        
        public void UpdateConfig(PlanetConfig config)
        {
            _config = config;
            _chunkGenerationPipeline.UpdateMin(config.origin);
            _chunkGenerationPipeline.UpdateSamplerSettings(config.samplerSettings);
            _chunkGenerationPipeline.UpdateChunkSize(config.chunkSize);
        }

        public void SplitChunkAt(ulong mortonCode)
        {
            Octree.Split(mortonCode, _config.samplerSettings);
        }
        
        public void MergeChunkAt(ulong mortonCode)
        {
            Octree.Merge(mortonCode);
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Data;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity;
using JetBrains.Annotations;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem
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

        [CanBeNull] private OctreeBuilder _octreeBuilder;
        public bool OctreeReady;
        private readonly Stopwatch _octreeBuilderStopwatch = new();
        public void RebuildOctree()
        {
            _octreeBuilder?.Dispose();  
            _octreeBuilder = new OctreeBuilder(_config.origin, _config.origin + new int3(_config.size), maxDepth: 5, _config.samplerSettings);
            _octreeBuilderStopwatch.Reset();
            _octreeBuilderStopwatch.Start();
            _octreeBuilder.Build();
            OctreeReady = false;
            
            _chunkGenerationPipeline.UpdateMinMax(_config.origin, _config.origin + new int3(_config.size));
            _chunkGenerationPipeline.UpdateSamplerSettings(_config.samplerSettings);
            _chunkGenerationPipeline.UpdateChunkSize(_config.chunkSize);
            ClearChunks();
        }

        public void Update()
        {
            _chunkGenerationPipeline.Update();
            if(!_octreeBuilder?.IsDone() ?? false) _octreeBuilder.Update();
            
            if ((!_octreeBuilder?.IsDone() ?? true) || OctreeReady) return;
            
            Octree? tree = _octreeBuilder.GetReadyTree();
            _octreeBuilderStopwatch.Stop();
            Debug.Assert(tree != null, "Octree is null!");
            
            Octree = tree.Value;
            _chunkGenerationPipeline.UpdateMaxDepth(Octree.MaxDepth);
                
            
            OctreeReady = true;
            Debug.Log("OCTREE READY, node count: " + Octree.Nodes.Length + ", Time elapsed: " + _octreeBuilderStopwatch.ElapsedMilliseconds + "ms \n ======================================");
            Debug.Log("Node Count of different depths: ");
            List<OctreeNode> octreeNodes = new();
            foreach (var t in Octree.Nodes)
            {
                octreeNodes.Add(t);
            }
            for (int i = 0; i < Octree.MaxDepth; i++)
            {
                Debug.Log($"Depth {i}: {octreeNodes.Count(node => node.MortonCode.GetDepth() == i)}");
            }
            
                
                
            foreach (OctreeNode octreeNode in Octree.Nodes)
            {
                _chunkGenerationPipeline.QueueGenerationAt(octreeNode.MortonCode);
            }
        }
        
        
        public void UpdateConfig(PlanetConfig config)
        {
            _config = config;
            _chunkGenerationPipeline.UpdateMinMax(config.origin, config.origin + new int3(_config.size));
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
            _octreeBuilder?.Dispose();
            _chunkGenerationPipeline.OnChunkGenerated -= OnChunkGenerated;
        }
    }
}
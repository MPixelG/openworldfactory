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
        private ChunkDataStore _chunkDataStore;
        
        private ChunkGenerationPipeline _chunkGenerationPipeline;
        
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
            _chunkGenerationPipeline.UpdateMaxDepth(Octree.MaxDepth);
            _chunkGenerationPipeline.UpdateSamplerSettings(_config.samplerSettings);
            OctreeReady = true;
            _chunkGenerationPipeline.QueueGenerationAt(new int3(0,0,0).EncodeToMorton(0));
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
            Debug.Log("Received generated chunk at morton code: " + chunkGeneration.MortonCode);
        }

        public void Dispose()
        {
            Octree.Dispose();
            _chunkGenerationPipeline.OnChunkGenerated -= OnChunkGenerated;
        }
    }
}
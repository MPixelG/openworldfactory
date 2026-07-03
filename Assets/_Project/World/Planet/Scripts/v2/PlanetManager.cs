using System;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.v2.Data;
using _Project.World.Planet.Scripts.v2.Unity;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.v2
{
    public class PlanetManager
    {
        private PlanetConfig _config;


        public Octree Octree { get; private set; }
        private ChunkDataStore _chunkDataStore;
        
        private ChunkGenerationPipeline _chunkGenerationPipeline;

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
            OctreeReady = true;
            
            _chunkGenerationPipeline.UpdateMaxDepth(Octree.MaxDepth);
            _chunkGenerationPipeline.UpdateSamplerSettings(_config.samplerSettings);
            _chunkGenerationPipeline.UpdateChunkSize(_config.chunkSize);
            
            _chunkGenerationPipeline.QueueGenerationAt(new int3(0,0,0).EncodeToMorton(1));
            _chunkGenerationPipeline.QueueGenerationAt(new int3(1,0,0).EncodeToMorton(1));
            _chunkGenerationPipeline.QueueGenerationAt(new int3(1,1,0).EncodeToMorton(1));
            _chunkGenerationPipeline.QueueGenerationAt(new int3(1,1,1).EncodeToMorton(1));
            _chunkGenerationPipeline.QueueGenerationAt(new int3(0,1,1).EncodeToMorton(1));
            _chunkGenerationPipeline.QueueGenerationAt(new int3(0,0,1).EncodeToMorton(1));
            _chunkGenerationPipeline.QueueGenerationAt(new int3(0,1,0).EncodeToMorton(1));
            _chunkGenerationPipeline.QueueGenerationAt(new int3(1,0,1).EncodeToMorton(1));
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

        public void Dispose()
        {
            Octree.Dispose();
            _chunkGenerationPipeline.OnChunkGenerated -= OnChunkGenerated;
        }
    }
}
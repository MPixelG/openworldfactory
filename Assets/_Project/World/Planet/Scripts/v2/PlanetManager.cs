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
        
        public PlanetManager(PlanetConfig config)
        {
            _config = config;
        }
        
        public bool OctreeReady {get; private set; }
        public void RebuildOctree()
        {
            Octree = OctreeHelper.Build(_config.origin, _config.origin + new int3(_config.size), _config.samplerSettings, 1);
            OctreeReady = true;
        }

        public void Update()
        {
            
            
            
        }
        
        
        public void UpdateConfig(PlanetConfig config)
        {
            _config = config;
        }

        public void SplitChunkAt(ulong mortonCode)
        {
            Octree.Split(mortonCode, _config.samplerSettings);
        }
        
        public void MergeChunkAt(ulong mortonCode)
        {
            Octree.Merge(mortonCode);
        }

        public void Dispose()
        {
            Octree.Dispose();
        }
    }
}
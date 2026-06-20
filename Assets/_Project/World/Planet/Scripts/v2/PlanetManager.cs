using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2
{
    public class PlanetManager
    {
        private PlanetConfig _config;
        
        
        private Octree _octree;
        private ChunkDataStore _chunkDataStore;


        public PlanetManager(PlanetConfig config)
        {
            _config = config;
        }
        
        public void UpdateConfig(PlanetConfig config)
        {
            _config = config;
        }

        public void SplitChunkAt(ulong mortonCode)
        {
            if (_octree.IndexLookup.TryGetValue(mortonCode, out int nodeIndex))
            {
                _octree.Split(nodeIndex, _config.samplerSettings);
            }
            else
            {
                Debug.LogWarning("No chunk with morton to split: " + mortonCode);
            }
        }

        public void Dispose()
        {
            _octree.Dispose();
        }
    }
}
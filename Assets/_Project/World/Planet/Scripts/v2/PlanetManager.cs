using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;

namespace _Project.World.Planet.Scripts.v2
{
    public class PlanetManager
    {
        private Octree _octree;
        
        private PlanetConfig _config;


        public PlanetManager(PlanetConfig config)
        {
            _config = config;
        }
        
        public void UpdateConfig(PlanetConfig config)
        {
            _config = config;
        }
    }
}
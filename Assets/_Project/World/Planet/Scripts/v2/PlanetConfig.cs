using _Project.World.Planet.Scripts.WorldGen;

namespace _Project.World.Planet.Scripts.v2
{
    [System.Serializable]
    public struct PlanetConfig
    {
        public int chunkSize;

        public BurstSamplerSettings samplerSettings;
    }
}
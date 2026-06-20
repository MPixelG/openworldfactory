using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.v2.Unity
{
    [System.Serializable]
    public struct PlanetConfig
    {
        public int chunkSize;
        public int3 origin;
        public float size;

        public BurstSamplerSettings samplerSettings;
    }
}
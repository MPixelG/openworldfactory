using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    [System.Serializable]
    public struct PlanetConfig
    {
        public byte chunkSize;
        public int3 origin;
        public float size;

        public ParallelBurstSamplerSettings samplerSettings;
        public MaterialDatabase materialDatabase;

    }
}
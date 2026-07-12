using _Project.World.Planet.Scripts.WorldGen;
using _Project.World.Planet.Scripts.WorldGen.Parallel;
using NUnit.Framework;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.v2.Unity
{
    [System.Serializable]
    public struct PlanetConfig
    {
        public byte chunkSize;
        public int3 origin;
        public float size;

        public ParallelBurstSamplerSettings samplerSettings;
    }
}
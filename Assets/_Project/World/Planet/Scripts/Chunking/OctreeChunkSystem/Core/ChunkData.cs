using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    public struct ChunkData
    {
        public ulong MortonCode;
        public byte LOD;

        public DensityFieldData Densities;
    }
}
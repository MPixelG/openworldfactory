using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.v2
{
    public struct ChunkData
    {
        public ulong MortonCode;
        public byte LOD;

        public DensityFieldData Densities;
        public MeshData MeshData;
    }
}
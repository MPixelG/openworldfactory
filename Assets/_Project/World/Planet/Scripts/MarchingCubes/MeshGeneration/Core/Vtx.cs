using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    internal struct Vtx
    {
        public float3 Pos;
        public VertexKey Key;
        public VoxelMaterial Material;
    }
}
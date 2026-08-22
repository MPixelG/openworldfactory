using _Project.World.Planet.Scripts.MarchingCubes.Materials;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public struct Triangle
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;
        public readonly VoxelMaterial Material;

        public Triangle(int a, int b, int c, VoxelMaterial material)
        {
            A = a;
            B = b;
            C = c;
            Material = material;
        }
    }
}
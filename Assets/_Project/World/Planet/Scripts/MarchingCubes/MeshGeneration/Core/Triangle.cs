using _Project.World.Planet.Scripts.MarchingCubes.Materials;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public struct Triangle
    {
        public int A;
        public int B;
        public int C;
        public VoxelMaterial Material;

        public Triangle(int a, int b, int c, VoxelMaterial material)
        {
            A = a;
            B = b;
            C = c;
            Material = material;
        }
    }
}
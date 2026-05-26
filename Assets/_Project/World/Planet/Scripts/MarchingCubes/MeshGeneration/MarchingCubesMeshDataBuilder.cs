using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public static class MarchingCubesMeshDataBuilder
    {
        public static MeshData GenerateMeshDataAt(int3 chunkPos, int size, DensityField densityField)
        {
            MeshDataBuilder meshDataBuilder = new MeshDataBuilder();
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        MarchingCubesMesher.GenerateAt((new int3(x, y, z)), densityField, meshDataBuilder);
                    }
                }
            }
            
            meshDataBuilder.NormalizeNormals();
            
            return meshDataBuilder.Build();
        }
        
    }
}
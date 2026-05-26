using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public static class DensityFieldBuilder
    {

        public static DensityField BuildDensityField(IDensitySampler generator, int size, int3 origin)
        {
            int gridSize = size + 1;
            float[] densities = new float[gridSize * gridSize * gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        float3 worldPos = new float3(origin.x + x, origin.y + y, origin.z + z);
                        float density = generator.DensityAt(worldPos);

                        densities[DensityField.IndexOf(x, y, z, gridSize)] = density;
                    }
                }
            }

            return new DensityField(densities, gridSize);
        }
    }
}
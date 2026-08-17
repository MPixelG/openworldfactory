using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.Dev.WorldGenDebug.Scripts
{
    /// <summary>
    /// 3D noise generator that reuses the same functions as the world generation system.
    /// Can be used to sample density and materials at any 3D position.
    /// </summary>
    public static class NoiseGenerator3D
    {
        /// <summary>
        /// Generates density and material at a world position using the same algorithm as BurstSphericalNoiseGenerator.
        /// </summary>
        public static (float density, VoxelMaterial material) GenerateAt(
            float3 worldPos,
            BurstSphericalNoiseConfig config)
        {
            // Directly use the 3D generation from the burst generator
            return BurstSphericalNoiseGenerator.GenerateAt(worldPos, config);
        }

        /// <summary>
        /// Generates multiple samples in a 3D region, useful for analyzing generation patterns.
        /// </summary>
        public static (float[], VoxelMaterial[]) GenerateRegion(
            float3 min,
            float3 max,
            int resolution,
            BurstSphericalNoiseConfig config)
        {
            int totalSamples = resolution * resolution * resolution;
            float[] densities = new float[totalSamples];
            VoxelMaterial[] materials = new VoxelMaterial[totalSamples];

            float3 size = max - min;

            for (int z = 0; z < resolution; z++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        float3 samplePos = min + new float3(
                            x / (float)resolution * size.x,
                            y / (float)resolution * size.y,
                            z / (float)resolution * size.z
                        );

                        var (density, material) = GenerateAt(samplePos, config);
                        int index = x + y * resolution + z * resolution * resolution;
                        densities[index] = density;
                        materials[index] = material;
                    }
                }
            }

            return (densities, materials);
        }
    }
}


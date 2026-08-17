using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Dev.WorldGenDebug.Scripts
{
    /// <summary>
    /// 2D Height map generator that uses the same noise functions as the 3D world generation.
    /// This is used for efficient real-time preview without needing to generate full 3D voxel data.
    /// </summary>
    public static class HeightMapGenerator2D
    {
        [BurstCompile]
        private struct BurstLatLongSamplingJob : IJobParallelFor
        {
            public int Width;
            public int Height;
            public BurstSphericalNoiseConfig Config;

            public NativeArray<float> Elevations;
            public NativeArray<VoxelMaterial> Materials;

            public void Execute(int index)
            {
                int x = index % Width;
                int y = index / Width;

                float u = Width > 1 ? x / (float)(Width - 1) : 0f;
                float v = Height > 1 ? y / (float)(Height - 1) : 0f;

                float latitude = math.lerp(90f, -90f, v);
                float longitude = math.lerp(-180f, 180f, u);

                float latRad = math.radians(latitude);
                float lonRad = math.radians(longitude);

                float cosLat = math.cos(latRad);
                float3 worldPos = new float3(
                    Config.Radius * cosLat * math.cos(lonRad),
                    Config.Radius * math.sin(latRad),
                    Config.Radius * cosLat * math.sin(lonRad)
                );

                var (density, material) = BurstSphericalNoiseGenerator.GenerateAt(worldPos, Config);
                Elevations[index] = -density;
                Materials[index] = material;
            }
        }

        public static void SampleHeightMapBurst(
            int width,
            int height,
            BurstSphericalNoiseConfig config,
            Allocator allocator,
            out NativeArray<float> elevations,
            out NativeArray<VoxelMaterial> materials)
        {
            int count = width * height;
            elevations = new NativeArray<float>(count, allocator);
            materials = new NativeArray<VoxelMaterial>(count, allocator);

            BurstLatLongSamplingJob job = new BurstLatLongSamplingJob
            {
                Width = width,
                Height = height,
                Config = config,
                Elevations = elevations,
                Materials = materials,
            };

            JobHandle handle = job.Schedule(count, 128);
            handle.Complete();
        }

        /// <summary>
        /// Generates a height map at a given latitude and longitude on a sphere.
        /// </summary>
        public static (float height, VoxelMaterial material) GenerateHeightAt(
            float latitude,
            float longitude,
            BurstSphericalNoiseConfig config)
        {
            // Convert spherical coordinates to 3D position on the sphere
            float latRad = latitude * Mathf.Deg2Rad;
            float lonRad = longitude * Mathf.Deg2Rad;

            float x = config.Radius * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
            float y = config.Radius * Mathf.Sin(latRad);
            float z = config.Radius * Mathf.Cos(latRad) * Mathf.Sin(lonRad);

            float3 worldPos = new float3(x, y, z);

            // Use the exact same 3D generator and derive surface elevation at the planet radius sample.
            var (density, material) = BurstSphericalNoiseGenerator.GenerateAt(worldPos, config);
            float elevation = -density;
            return (elevation, material);
        }

        /// <summary>
        /// Generates a complete height map texture for visualization.
        /// </summary>
        public static Texture2D GenerateHeightMapTexture(
            int width,
            int height,
            BurstSphericalNoiseConfig config,
            bool showMaterialDifferences = true)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[width * height];

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            SampleHeightMapBurst(
                width,
                height,
                config,
                Allocator.TempJob,
                out NativeArray<float> sampledHeights,
                out NativeArray<VoxelMaterial> sampledMaterials
            );

            for (int i = 0; i < sampledHeights.Length; i++)
            {
                float h = sampledHeights[i];
                if (h < minHeight)
                    minHeight = h;
                if (h > maxHeight)
                    maxHeight = h;
            }

            float range = math.max(maxHeight - minHeight, 1e-5f);

            for (int i = 0; i < pixels.Length; i++)
            {
                if (showMaterialDifferences)
                {
                    pixels[i] = GetMaterialColor(sampledMaterials[i]);
                }
                else
                {
                    float normalizedHeight = math.saturate((sampledHeights[i] - minHeight) / range);
                    byte heightByte = (byte)(normalizedHeight * 255f);
                    pixels[i] = new Color32(heightByte, heightByte, heightByte, 255);
                }
            }

            sampledHeights.Dispose();
            sampledMaterials.Dispose();

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Gets a color representing the voxel material type.
        /// </summary>
        private static Color32 GetMaterialColor(VoxelMaterial material)
        {
            return material switch
            {
                VoxelMaterial.Air => new Color32(135, 206, 235, 255),        // Sky blue
                VoxelMaterial.Dirt => new Color32(139, 69, 19, 255),         // Brown
                VoxelMaterial.Stone => new Color32(128, 128, 128, 255),      // Gray
                VoxelMaterial.Water => new Color32(30, 144, 255, 255),       // Dodger blue
                VoxelMaterial.Sand => new Color32(238, 214, 175, 255),       // Wheat
                VoxelMaterial.Grass => new Color32(34, 139, 34, 255),        // Forest green
                VoxelMaterial.Snow => new Color32(255, 250, 250, 255),       // Snow white
                VoxelMaterial.Lava => new Color32(255, 69, 0, 255),          // Red-orange
                _ => new Color32(255, 255, 255, 255)
            };
        }
    }
}


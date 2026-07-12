using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Parallel
{
    [CreateAssetMenu(menuName = "WorldGen/Burst Density Samplers/Spherical Noise")]
    public class ParallelBurstSamplerSettings : ScriptableObject
    {
        public float radius;

        public BurstSphericalNoiseSamplingJob CreateExactSampler(
            NativeList<ulong> mortons,
            int3 origin, byte resolution, byte maxDepth,
            out DensityFieldData densityField)
        {
            densityField = new DensityFieldData
            {
                Size = resolution * resolution * resolution,
                Densities = new NativeArray<float>(resolution * resolution * resolution,
                    Allocator.Persistent)
            };

            return new BurstSphericalNoiseSamplingJob()
            {
                Resolution = resolution,
                MortonCodes = mortons,
                Origin = origin,
                maxDepth = maxDepth,
                Densities = densityField.Densities,

                Config = new BurstSphericalNoiseConfig()
                {
                    Radius = radius,
                    ReferenceRadius = 200f,

                    TerrainHeight = 169f,

                    ContinentFrequency = 1.2f,
                    ContinentOctaves = 2,
                    ContinentPersistence = 0.5f,

                    OceanThreshold = 0.48f,

                    MountainMaskFrequency = 3f,
                    MountainThreshold = 0.82f,
                    MountainBlend = 0.32f,

                    MountainFrequency = 7f,
                    MountainOctaves = 3,
                    MountainPersistence = 0.6f,
                    MountainSharpness = 15.5f,

                    PlainsStrength = 2.5f,
                    PlainsFrequency = 3f,

                    DetailFrequency = 30f,
                    DetailStrength = 1.8f,

                    WarpFrequency = 0.6f,
                    WarpStrength = 0.3f,
                }
            };
        }

        public BurstSphericalNoiseClassificationJob CreateMinMaxSamplers(
            NativeList<ulong> mortons,
            int3 origin, byte resolution, byte maxDepth,
            ref NativeArray<MinMaxValue> minMaxValues)
        {
            return new BurstSphericalNoiseClassificationJob
            {
                Origin = origin,

                Resolution = resolution,
                
                
                MortonCodes = mortons,
                Results = minMaxValues, 
                
                maxDepth = maxDepth,
                

                Config = new BurstSphericalNoiseConfig()
                {
                    Radius = radius,
                    ReferenceRadius = 200f,

                    TerrainHeight = 169f,

                    ContinentFrequency = 1.2f,
                    ContinentOctaves = 2,
                    ContinentPersistence = 0.5f,

                    OceanThreshold = 0.48f,

                    MountainMaskFrequency = 3f,
                    MountainThreshold = 0.82f,
                    MountainBlend = 0.32f,

                    MountainFrequency = 7f,
                    MountainOctaves = 3,
                    MountainPersistence = 0.6f,
                    MountainSharpness = 15.5f,

                    PlainsStrength = 2.5f,
                    PlainsFrequency = 3f,

                    DetailFrequency = 30f,
                    DetailStrength = 1.8f,

                    WarpFrequency = 0.6f,
                    WarpStrength = 0.3f,
                }
            };
        }
    }
}
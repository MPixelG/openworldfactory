using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    [CreateAssetMenu(menuName = "WorldGen/Burst Density Samplers/Spherical Noise")]

    public class BurstSamplerSettings : ScriptableObject
    {
        public float radius;
        
        public BurstSphericalNoiseSamplerJob CreateExactSampler(int3 min, int3 max, byte resolution, out DensityFieldData densityField)
        {
            densityField = new DensityFieldData
            {
                Size = resolution * resolution * resolution,
                Densities = new Unity.Collections.NativeArray<float>(resolution * resolution * resolution,
                    Unity.Collections.Allocator.Persistent)
            };
            
            return new BurstSphericalNoiseSamplerJob
            {
                JobType = DensitySamplingJobType.Exact,
                
                Densities = densityField.Densities,
                
                MinPos = min,
                MaxPos = max,
                Resolution = resolution,
                
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
            };
        }
        
        public BurstSphericalNoiseSamplerJob CreateMinMaxSampler(int3 min, int3 max, byte resolution, out MinMaxValue minMaxValue)
        {
            minMaxValue = new MinMaxValue();
            return new BurstSphericalNoiseSamplerJob
            {
                JobType = DensitySamplingJobType.MinMax,
                
                MinMaxValue = minMaxValue, 
                
                MinPos = min,
                MaxPos = max,
                Resolution = resolution,
                
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
            };
        }
        
    }
}
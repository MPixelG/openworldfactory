using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    [CreateAssetMenu(menuName = "WorldGen/Burst Density Samplers/Spherical Noise")]
    public class ParallelBurstSamplerSettings : ScriptableObject
    {
        public float radius;

        private static BurstSphericalNoiseConfig GetDefaultConfig(float radius) => new()
        {
            Radius = radius,

            TerrainHeight = 0.17f,
            
            ContinentOctaves = 7,
            ContinentPersistence = 0.5f,
            ContinentFrequency = 0.005f,
            
            MountainMaskFrequency = 0.02f,
            MountainThreshold = 0.72f,
            MountainBlend = 0.3f, 

            MountainFrequency = 0.002f,
            MountainOctaves = 12,
            MountainPersistence = 0.45f,
            MountainSharpness = 1f,

            PlainsStrength = 20f,
            PlainsFrequency = 0.01f,

            DetailFrequency = 0.3f,
            DetailStrength = 0.008f,

            WarpFrequency = 0.01f,
            WarpStrength = 1f,
        };
        
        public ParallelBurstSphericalNoiseSamplingJob CreateParallelExactSampler(
            float3 minPos, float3 maxPos, byte resolution,
            out FieldData field)
        {
            field = new FieldData
            {
                Size = resolution,
                Fields = new NativeArray<Voxel>(resolution * resolution * resolution,
                    Allocator.Persistent)
            };

            return new ParallelBurstSphericalNoiseSamplingJob()
            {
                Resolution = resolution,
                Min = minPos,
                Max = maxPos,
                Fields = field.Fields,

                Config = GetDefaultConfig(radius)
            };
        }
        

        public BurstSphericalNoiseClassificationJob CreateMinMaxSamplers(
            NativeList<ulong> mortons,
            float3 min, float3 max, byte resolution, byte maxDepth,
            ref NativeArray<MinMaxValue> minMaxValues)
        {
            return new BurstSphericalNoiseClassificationJob
            {
                Min = min,
                Max = max,
                

                Resolution = resolution,
                
                
                MortonCodes = mortons,
                Results = minMaxValues, 
                
                MaxDepth = maxDepth,
                

                Config = GetDefaultConfig(radius)
            };
        }
    }
}
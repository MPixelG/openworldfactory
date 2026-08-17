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

            TerrainHeight = 0.428f,
            
            ContinentOctaves = 5,
            ContinentPersistence = 0.6f,
            ContinentFrequency = 0.014f,
            
            MountainMaskFrequency = 0f,
            MountainThreshold = 0f,
            MountainBlend = 0f, 

            MountainFrequency = 0.0086f,
            MountainOctaves = 8,
            MountainPersistence = 0.575f,
            MountainSharpness = 5f,

            PlainsStrength = 0f,
            PlainsFrequency = 0f,

            DetailFrequency = 0f,
            DetailStrength = 0f,

            WarpFrequency = 0.0053f,
            WarpStrength = 0.32f,
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
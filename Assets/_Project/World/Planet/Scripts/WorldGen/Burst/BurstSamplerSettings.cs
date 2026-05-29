using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Burst
{
    [CreateAssetMenu(menuName = "WorldGen/Burst Density Samplers/Spherical Noise")]

    public class BurstSamplerSettings : ScriptableObject
    {
        public float radius;
        
        public BurstSphericalNoiseSamplerJob CreateSampler(int chunkSize, int3 origin)
        {
            return new BurstSphericalNoiseSamplerJob
            {
                Origin = origin,
                Radius = radius,
                Size = chunkSize,
                
                ReferenceRadius = 200f,

                TerrainHeight = 169f,

                ContinentFrequency = 1.2f,
                ContinentOctaves = 8,
                ContinentPersistence = 0.5f,

                OceanThreshold = 0.48f,

                MountainMaskFrequency = 3f,
                MountainThreshold = 0.82f,
                MountainBlend = 0.32f,

                MountainFrequency = 7f,
                MountainOctaves = 12,
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
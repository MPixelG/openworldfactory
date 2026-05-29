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
            };
        }
    }
}
using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    [CreateAssetMenu(menuName = "WorldGen/TerrainGenerator")] [Serializable]
    public class TerrainNoiseGenerator : ScriptableObject, IDensitySampler
    {

        [SerializeField] private float noiseFrequency = 0.05f;
        [SerializeField] private float noiseAmplitude = 0.5f;
        [SerializeField] private float noiseBias = 0.8f;

        public float DensityAt(float3 position)
        {
            float rawNoise = noise.cnoise(position * noiseFrequency); // get some noise at the current world position, scaled by the frequency
            
            return noiseAmplitude * rawNoise + noiseBias;
        }
    }
}
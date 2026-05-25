using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    [CreateAssetMenu(menuName = "WorldGen/TerrainGenerator")] [Serializable]
    public class TerrainNoiseGenerator : TerrainGenerator
    {

        [SerializeField] private float noiseFrequency;
        [SerializeField] private float noiseAmplitude;
        [SerializeField] private float noiseBias;
        
        public override float DensityAt(Vector3 worldPosition)
        {
            float rawNoise = noise.cnoise(worldPosition * noiseFrequency); // get some noise at the current world position, scaled by the frequency
            
            return noiseAmplitude * rawNoise + noiseBias;
        }
    }
}
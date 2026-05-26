using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// a combination of the SphereGenerator and a noise generator. it creates a sphere with noise on top of it.
    /// </summary>
    [CreateAssetMenu(menuName = "WorldGen/SphericalNoiseGenerator")] [Serializable]
    public class SphericalNoiseSampler : ScriptableObject, IDensitySampler
    {
        
        [SerializeField, Range(0.001f, 1f)] private float noiseFrequency; // the frequency of the noise. it scales the input to the noise function so lower frequency = less noise, higher frequency = more noise
        [SerializeField, Range(0.01f, 1f)] private float noiseAmplitude; // the amount of noise that gets added
        [SerializeField, Range(0f, 1f)] private float noiseBias; // a minimum noise value that gets added so that the sphere always has a fraction of its size, even if the noise is negative
        [SerializeField, Range(0f, 1f)] private float radius;
        [SerializeField] private float3 center;


        public float DensityAt(float3 position)
        {
            float3 delta = position - center;

            float dist = math.length(delta);

            float sphereSdf = dist - radius;

            float3 dir = math.normalizesafe(delta, new float3(0f, 1f, 0f));

            float rawNoise = noise.cnoise(dir * noiseFrequency * radius);

            float noiseValue = Math.Clamp(rawNoise, -100, 100) * noiseAmplitude * radius;

            return sphereSdf - noiseValue;
        }
    }
}
using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Samplers
{
    /// <summary>
    /// a combination of the SphereGenerator and a noise generator. it creates a sphere with noise on top of it.
    /// </summary>
    public class SphericalNoiseSampler : IDensitySampler
    {
        
        private float _noiseFrequency; // the frequency of the noise. it scales the input to the noise function so lower frequency = less noise, higher frequency = more noise
        private float _noiseAmplitude; // the amount of noise that gets added
        private float _noiseBias; // a minimum noise value that gets added so that the sphere always has a fraction of its size, even if the noise is negative
        private float _radius;
        private float3 _center;


        public float DensityAt(float3 position)
        {
            float3 delta = position - _center;

            float dist = math.length(delta);

            float sphereSdf = dist - _radius;

            float3 dir = math.normalizesafe(delta, new float3(0f, 1f, 0f));

            float rawNoise = noise.cnoise(dir * _noiseFrequency * _radius);

            float noiseValue = Math.Clamp(rawNoise, -100, 100) * _noiseAmplitude * _radius;

            return sphereSdf - noiseValue;
        }
    }
}
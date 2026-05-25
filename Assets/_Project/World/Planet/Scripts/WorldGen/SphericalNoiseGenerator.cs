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
    public class SphericalNoiseGenerator : SphereGenerator
    {
        
        [SerializeField, Range(0.001f, 1f)] private float noiseFrequency; // the frequency of the noise. it scales the input to the noise function so lower frequency = less noise, higher frequency = more noise
        [SerializeField, Range(0.01f, 1f)] private float noiseAmplitude; // the amount of noise that gets added
        [SerializeField, Range(0f, 1f)] private float noiseBias; // a minimum noise value that gets added so that the sphere always has a fraction of its size, even if the noise is negative

        
        public SphericalNoiseGenerator(Vector3 center, float radius, float noiseFrequency, float noiseAmplitude, float noiseBias) : base(center, radius)
        {
            this.noiseFrequency = noiseFrequency;
            this.noiseAmplitude = noiseAmplitude;
            this.noiseBias = noiseBias;
        }


        private Dictionary<int3, float> _noiseCache = new(); // a cache for the noise values at different angles to the center



        public override float DensityAt(Vector3 worldPosition)
        {
            float3 delta = worldPosition - center;

            float dist = math.length(delta);

            float sphereSdf = dist - radius;

            float3 dir = delta / dist;

            float rawNoise = noise.cnoise(dir * noiseFrequency * radius);

            float noiseValue = Math.Clamp(rawNoise, -100, 100) * noiseAmplitude * radius;

            return sphereSdf - noiseValue;
        }


        protected override void OnValidate()
        {
            _noiseCache ??= new Dictionary<int3, float>();
            _noiseCache.Clear();
            base.OnValidate();
        }
    }
}
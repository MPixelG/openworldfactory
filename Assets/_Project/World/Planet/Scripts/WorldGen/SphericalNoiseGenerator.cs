using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// a combination of the SphereGenerator and a noise generator. it creates a sphere with noise on top of it.
    /// </summary>
    [CreateAssetMenu(menuName = "WorldGen/SphericalNoiseGenerator")] [Serializable]
    public class SphericalNoiseGenerator : SphereGenerator
    {
        
        [SerializeField, Range(0.01f, 1f)] private float noiseFrequency; // the frequency of the noise. it scales the input to the noise function so lower frequency = less noise, higher frequency = more noise
        [SerializeField, Range(0.01f, 3f)] private float noiseAmplitude; // the amount of noise that gets added
        [SerializeField, Range(0f, 1f)] private float noiseBias; // a minimum noise value that gets added so that the sphere always has a fraction of its size, even if the noise is negative

        
        public SphericalNoiseGenerator(Vector3 center, float radius, float noiseFrequency, float noiseAmplitude, float noiseBias) : base(center, radius)
        {
            this.noiseFrequency = noiseFrequency;
            this.noiseAmplitude = noiseAmplitude;
            this.noiseBias = noiseBias;
        }


        public override float DensityAt(Vector3 worldPosition, Vector3 center)
        {
            float sphereSdf = base.DensityAt(worldPosition, center);

            float rawNoise = noise.cnoise(worldPosition * noiseFrequency);

            float noiseValue = rawNoise * noiseAmplitude;
            
            return sphereSdf * (noiseValue + noiseBias);
        }
    }
}
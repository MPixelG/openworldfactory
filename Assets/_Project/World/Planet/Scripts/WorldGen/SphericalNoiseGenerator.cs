using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// a combination of the SphereGenerator and a noise generator. it creates a sphere with noise on top of it.
    /// </summary>
    public class SphericalNoiseGenerator : SphereGenerator
    {
        
        private readonly float _noiseFrequency; // the frequency of the noise. it scales the input to the noise function so lower frequency = less noise, higher frequency = more noise
        private readonly float _noiseAmplitude; // the amount of noise that gets added
        private readonly float _noiseBias; // a minimum noise value that gets added so that the sphere always has a fraction of its size, even if the noise is negative

        
        public SphericalNoiseGenerator(Vector3 center, float radius, float noiseFrequency, float noiseAmplitude, float noiseBias) : base(center, radius)
        {
            _noiseFrequency = noiseFrequency;
            _noiseAmplitude = noiseAmplitude;
            _noiseBias = noiseBias;
        }


        public override float DensityAt(Vector3 worldPosition)
        {
            float sphereSdf = base.DensityAt(worldPosition);

            float rawNoise = noise.cnoise(worldPosition * _noiseFrequency);

            float noiseValue = rawNoise * _noiseAmplitude;
            
            return sphereSdf * (noiseValue + _noiseBias);
        }
    }
}
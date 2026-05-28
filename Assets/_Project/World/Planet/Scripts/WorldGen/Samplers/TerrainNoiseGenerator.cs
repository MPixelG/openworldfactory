using System;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen.Samplers
{
    [Serializable]
    public class TerrainNoiseGenerator : IDensitySampler
    {
        private readonly float _noiseFrequency;
        private readonly float _noiseAmplitude;
        private readonly float _noiseBias;

        public TerrainNoiseGenerator()
            : this(0.05f, 0.5f, 0.8f)
        {
        }

        public TerrainNoiseGenerator(float noiseFrequency, float noiseAmplitude, float noiseBias)
        {
            _noiseFrequency = noiseFrequency;
            _noiseAmplitude = noiseAmplitude;
            _noiseBias = noiseBias;
        }

        public float DensityAt(float3 position)
        {
            float rawNoise = noise.cnoise(position * _noiseFrequency); // get some noise at the current world position, scaled by the frequency

            return _noiseAmplitude * rawNoise + _noiseBias;
        }
    }
}
using _Project.World.Planet.Scripts.WorldGen.Samplers;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Unity
{
    [CreateAssetMenu(menuName = "WorldGen/Density Samplers/Terrain Noise")]
    public class TerrainNoiseGeneratorSettings : DensitySamplerSettings
    {
        [SerializeField] private float noiseFrequency = 0.05f;
        [SerializeField] private float noiseAmplitude = 0.5f;
        [SerializeField] private float noiseBias = 0.8f;

        public override IDensitySampler CreateSampler()
        {
            return new TerrainNoiseGenerator(noiseFrequency, noiseAmplitude, noiseBias);
        }
    }
}

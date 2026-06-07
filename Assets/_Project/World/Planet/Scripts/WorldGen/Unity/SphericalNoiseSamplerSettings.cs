using _Project.World.Planet.Scripts.WorldGen.Samplers;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Unity
{
    [CreateAssetMenu(menuName = "WorldGen/Density Samplers/Spherical Noise")]
    public class SphericalNoiseSamplerSettings : DensitySamplerSettings
    {
        [SerializeField] private float radius = 10f;
        [SerializeField] private Vector3 center = Vector3.zero;
        [SerializeField] private float noiseFrequency = 0.05f;
        [SerializeField] private float noiseAmplitude = 0.5f;
        [SerializeField] private float noiseBias = 0.8f;

        public override IDensitySampler CreateSampler()
        {
            return new SphericalNoiseSampler(center, radius, noiseFrequency, noiseAmplitude, noiseBias);
        }
    }
}

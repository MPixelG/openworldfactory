using _Project.World.Planet.Scripts.WorldGen.Samplers;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Unity
{
    [CreateAssetMenu(menuName = "WorldGen/Density Samplers/Sphere")]
    public class SphereSamplerSettings : DensitySamplerSettings
    {
        [SerializeField] private float radius = 10f;
        [SerializeField] private Vector3 center = Vector3.zero;

        public override IDensitySampler CreateSampler()
        {
            return new SphereSampler(center, radius);
        }
    }
}

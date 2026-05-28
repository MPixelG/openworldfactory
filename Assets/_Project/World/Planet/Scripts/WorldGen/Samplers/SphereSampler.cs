using System;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen.Samplers
{
    /// <summary>
    /// A simple terrain generator that creates a spherical planet
    /// </summary>
    [Serializable]
    public class SphereSampler : IDensitySampler
    {
        private readonly float _radius;
        private readonly float3 _center;

        public SphereSampler()
            : this(float3.zero, 10f)
        {
        }

        public SphereSampler(float3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }

        public virtual float DensityAt(float3 position) => math.distance(position, _center) - _radius;
    }
}

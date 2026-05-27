using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Samplers
{
    /// <summary>
    /// A simple terrain generator that creates a spherical planet
    /// </summary>
    [Serializable]
    public class SphereSampler : IDensitySampler
    {
        public float radius;

        public float3 center;

        public SphereSampler()
        {
            radius = 10;
        }

        public SphereSampler(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public virtual float DensityAt(float3 position) => Vector3.Distance(position, center) - radius;
    }
}

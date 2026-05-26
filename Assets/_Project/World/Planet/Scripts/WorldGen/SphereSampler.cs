using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// A simple terrain generator that creates a spherical planet
    /// </summary>
    [CreateAssetMenu(menuName = "WorldGen/SphericalGenerator")] [Serializable]
    public class SphereSampler : ScriptableObject, IDensitySampler
    {
        [SerializeField] protected float radius;
        
        [SerializeField] protected float3 center;

        protected SphereSampler()
        {
            radius = 10;
        }

        protected SphereSampler(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
        
        public virtual float DensityAt(float3 position) => Vector3.Distance(position, center) - radius;
    }
}
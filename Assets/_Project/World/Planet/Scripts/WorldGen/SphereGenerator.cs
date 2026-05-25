using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// A simple terrain generator that creates a spherical planet
    /// </summary>
    [CreateAssetMenu(menuName = "WorldGen/SphericalGenerator")] [Serializable]
    public class SphereGenerator : TerrainGenerator
    {
        [SerializeField] protected float radius;
        
        [SerializeField] protected Vector3 center;

        protected SphereGenerator()
        {
            radius = 10;
        }

        protected SphereGenerator(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
        
        public override float DensityAt(Vector3 worldPosition) => Vector3.Distance(worldPosition, center) - radius;
    }
}
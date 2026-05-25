using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// A simple terrain generator that creates a spherical planet
    /// </summary>
    public class SphereGenerator : TerrainGenerator
    {
        private readonly Vector3 _center;
        private readonly float _radius;

        protected SphereGenerator(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }
        
        public override float DensityAt(Vector3 worldPosition) => _radius - Vector3.Distance(worldPosition, _center);
    }
}
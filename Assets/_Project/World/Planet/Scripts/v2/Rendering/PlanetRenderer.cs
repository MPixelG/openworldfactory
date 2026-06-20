using System;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2.Rendering
{
    public class PlanetRenderer : MonoBehaviour
    {
        private PlanetManager _planetManager;
        
        private FrustumCullingSystem _frustumCullingSystem;
        
        [SerializeField] private Camera viewer;
        
        public void SetPlanetManager(PlanetManager planetManager)
        {
            _planetManager = planetManager;
        }

        private void Awake()
        {
            _frustumCullingSystem = new FrustumCullingSystem();
        }

        
        private void Update()
        {
            _frustumCullingSystem.Update(
                _planetManager.Octree,
                viewer
            );
        }

        private void OnDestroy()
        {
            _planetManager.Dispose();
        }
    }
}
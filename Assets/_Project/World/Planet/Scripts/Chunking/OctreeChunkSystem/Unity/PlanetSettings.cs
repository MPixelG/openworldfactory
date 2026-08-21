using System;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class PlanetSettings : MonoBehaviour
    {
        public PlanetConfig config;
        
        public event Action<PlanetConfig> OnSettingsChanged;

        private void OnValidate()
        {
            OnSettingsChanged?.Invoke(config);
        }
    }
}
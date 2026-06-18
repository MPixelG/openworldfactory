using System;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2.Unity
{
    public class PlanetSettings : ScriptableObject
    {
        [SerializeField] private PlanetConfig config;
        
        public event Action<PlanetConfig> OnSettingsChanged;
        public PlanetConfig Config => config;

        private void OnValidate()
        {
            OnSettingsChanged?.Invoke(config);
        }
    }
}
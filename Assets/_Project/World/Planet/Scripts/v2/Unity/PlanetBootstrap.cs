using UnityEngine;

namespace _Project.World.Planet.Scripts.v2.Unity
{
    public class PlanetBootstrap : MonoBehaviour
    {
        [SerializeField] private PlanetSettings settings;

        private PlanetManager _planetManager;

        private void Awake()
        {
            _planetManager = new PlanetManager(settings.Config);
        }


        private void OnEnable()
        {
            if(settings != null) settings.OnSettingsChanged += HandleSettingsChanged;
        }

        private void OnDisable()
        {
            if(settings != null) settings.OnSettingsChanged -= HandleSettingsChanged;
        }

        private void HandleSettingsChanged(PlanetConfig config)
        {
            _planetManager.UpdateConfig(config);
        }
        


    }
}
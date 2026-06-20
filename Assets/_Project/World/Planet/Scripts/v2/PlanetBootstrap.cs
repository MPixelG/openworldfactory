/*
using _Project.World.Planet.Scripts.v2.Unity;
using UnityEditor;
using UnityEngine;
using PlanetBootstrap = _Project.World.Planet.Scripts.v2.PlanetBootstrap;

namespace _Project.World.Planet.Scripts.v2
{
    public class PlanetBootstrap : MonoBehaviour
    {
        [SerializeField] private new PlanetRenderer renderer;
        [SerializeField] private PlanetSettings settings;

        private PlanetManager _manager;


        private void OnEnable() // this is called once when the component is applied or the unity prefab is instantiated
        {
            if (renderer == null) // only add a renderer if the current one is not set
            {
                renderer = GetComponent<PlanetRenderer>();
            }

            if (settings ==
                null) // the user has to specify the settings themselves so if the settings arent specified when the object is instantiated we throw an error 
            {
                Debug.LogError("OctreeChunkSystemSettings is missing.", this);
                return;
            }

            if (renderer == null) // same goes for the renderer
            {
                Debug.LogError("OctreeChunkSystemRenderer is missing.", this);
                return;
            }

            _manager = new PlanetManager(settings.config);
        
            renderer.SetPlanetManager(_manager); // apply the chunk manager to the renderer
        }

        private void OnDisable() // this is called when the component is removed or the unity prefab is destroyed
        {
            if (renderer != null) // if there is a renderer assigned we need to dispose it and its components to prevent memory leaks and other issues
            {
                renderer.SetPlanetManager(null); // "remove" the chunk manager
            }

            _manager = null; // remove the chunk manager object reference
        }

        public void RebuildChunks()
        {
            _manager.UpdateConfig(settings.config);
            _manager.RebuildOctree();
        }

    }
}
*/


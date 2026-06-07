using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem.Unity
{
    /// <summary>
    /// the chunk system bootstrap is the unity component that manages the chunks. you have to apply the renderer that renders the chunks and a settings object for further customization. a viewer is also required for streaming the chunks around that object. the main camera is recommended. 
    /// </summary>
    public class GridChunkSystemBootstrap : MonoBehaviour
    {
        [SerializeField] private GridChunkSystemSettings settings;
        [SerializeField] private new GridChunkSystemRenderer renderer;
        [SerializeField] private Transform viewer;

        private GridChunkManager _chunkManager;

        private void OnEnable() // this is called once when the component is applied or the unity prefab is instantiated
        {
            if (renderer == null) // only add a renderer if the current one is not set
            {
                renderer = GetComponent<GridChunkSystemRenderer>(); 
            }

            if (settings == null) // the user has to specify the settings themselves so if the settings arent specified when the object is instantiated we throw an error 
            {
                Debug.LogError("GridChunkSystemSettings is missing.", this);
                return;
            }

            if (renderer == null) // same goes for the renderer
            {
                Debug.LogError("GridChunkSystemRenderer is missing.", this);
                return;
            }

            _chunkManager = settings.CreateManager(); // create the chunk manager based on the settings. this is where the logic of the chunk system is being created.
            renderer.SetChunkManager(_chunkManager); // apply the chunk manager to the renderer
            renderer.SetViewer(viewer);
        }

        private void OnDisable() // this is called when the component is removed or the unity prefab is destroyed
        {
            if (renderer != null) // if there is a renderer assigned we need to dispose it and its components to prevent memory leaks and other issues
            {
                renderer.SetChunkManager(null); // "remove" the chunk manager
                renderer.SetViewer(null); // and viewer
            }

            _chunkManager = null; // remove the chunk manager object reference
        }
    }
}

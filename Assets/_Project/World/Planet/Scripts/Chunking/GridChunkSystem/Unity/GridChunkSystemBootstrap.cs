using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem.Unity
{
    [ExecuteAlways]
    public class GridChunkSystemBootstrap : MonoBehaviour
    {
        [SerializeField] private GridChunkSystemSettings settings;
        [SerializeField] private new GridChunkSystemRenderer renderer;
        [SerializeField] private Transform viewer;

        private GridChunkManager _chunkManager;

        private void OnEnable()
        {
            if (renderer == null)
            {
                renderer = GetComponent<GridChunkSystemRenderer>();
            }

            if (settings == null)
            {
                Debug.LogError("GridChunkSystemSettings is missing.", this);
                return;
            }

            if (renderer == null)
            {
                Debug.LogError("GridChunkSystemRenderer is missing.", this);
                return;
            }

            _chunkManager = settings.CreateManager();
            renderer.SetChunkManager(_chunkManager);
            renderer.SetViewer(viewer);
        }

        private void OnDisable()
        {
            if (renderer != null)
            {
                renderer.SetChunkManager(null);
                renderer.SetViewer(null);
            }

            _chunkManager = null;
        }
    }
}

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class OctreeChunkSystemBootstrap : MonoBehaviour
    {
        [SerializeField] private OctreeChunkSystemSettings settings;
        [SerializeField] private new OctreeChunkSystemRenderer renderer;
        [SerializeField] private Transform viewer;

        private OctreeChunkManager _chunkManager;

        private void OnEnable() // this is called once when the component is applied or the unity prefab is instantiated
        {
            if (renderer == null) // only add a renderer if the current one is not set
            {
                renderer = GetComponent<OctreeChunkSystemRenderer>(); 
            }

            if (settings == null) // the user has to specify the settings themselves so if the settings arent specified when the object is instantiated we throw an error 
            {
                Debug.LogError("OctreeChunkSystemSettings is missing.", this);
                return;
            }

            if (renderer == null) // same goes for the renderer
            {
                Debug.LogError("OctreeChunkSystemRenderer is missing.", this);
                return;
            }

            _chunkManager = settings.CreateManager(); // create the chunk manager based on the settings. this is where the logic of the chunk system is being created.
            renderer.SetChunkManager(_chunkManager); // apply the chunk manager to the renderer
        }

        private void OnDisable() // this is called when the component is removed or the unity prefab is destroyed
        {
            if (renderer != null) // if there is a renderer assigned we need to dispose it and its components to prevent memory leaks and other issues
            {
                renderer.SetChunkManager(null); // "remove" the chunk manager
            }

            _chunkManager = null; // remove the chunk manager object reference
        }

        public void RebuildChunks()
        {
            _chunkManager.Origin = new int3(settings.origin.x, settings.origin.y, settings.origin.z);
            _chunkManager.Size = settings.size;
            _chunkManager.RebuildOctree();
        }
    }
    
    
#if UNITY_EDITOR

    [CustomEditor(typeof(OctreeChunkSystemBootstrap))]
    public class OctreeChunkSystemRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!GUILayout.Button("Rebuild Chunks")) return;
            
            var renderer = (OctreeChunkSystemBootstrap) target;
            renderer.RebuildChunks();
            
            

            EditorUtility.SetDirty(renderer);
        }
    }
#endif
}
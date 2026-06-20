using _Project.World.Planet.Scripts.v2.Unity;
using UnityEditor;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2.Unity
{
    public class PlanetBootstrap : MonoBehaviour
    {
        [SerializeField] private PlanetSettings settings;
        [SerializeField] private new PlanetRenderer renderer;

        private PlanetManager _planetManager;


        private void OnEnable()
        {
            _planetManager = new PlanetManager(settings.config);
            _planetManager.RebuildOctree();
            
            if(settings != null) settings.OnSettingsChanged += HandleSettingsChanged;
            
            renderer.SetPlanetManager(_planetManager); // apply the chunk manager to the renderer
            Debug.Log("Planet manager set: " + _planetManager);
        }

        private void OnDisable()
        {
            if(settings != null) settings.OnSettingsChanged -= HandleSettingsChanged;
        }

        private void HandleSettingsChanged(PlanetConfig config)
        {
            _planetManager.UpdateConfig(config);
        }

        public void RebuildOctree()
        {
            Debug.Log("Rebuilding Octree!! planet manager: " + _planetManager);
            _planetManager.RebuildOctree();
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(PlanetBootstrap))]
public class PlanetRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!GUILayout.Button("Rebuild Chunks")) return;
            
        var bootstrap = (PlanetBootstrap) target;
        bootstrap.RebuildOctree();
        
        EditorUtility.SetDirty(bootstrap);
    }
}
#endif

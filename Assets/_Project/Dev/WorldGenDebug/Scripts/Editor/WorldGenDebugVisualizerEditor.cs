#if UNITY_EDITOR
using _Project.Dev.WorldGenDebug.Scripts;
using _Project.World.Planet.Scripts.WorldGen;
using UnityEditor;
using UnityEngine;

namespace _Project.Dev.WorldGenDebug.Scripts.Editor
{
    /// <summary>
    /// Custom editor for WorldGenDebugVisualizer that adds preset options.
    /// </summary>
    [CustomEditor(typeof(WorldGenDebugVisualizer))]
    public class WorldGenDebugVisualizerEditor : UnityEditor.Editor
    {
        private WorldGenDebugVisualizer _visualizer;

        private void OnEnable()
        {
            _visualizer = (WorldGenDebugVisualizer)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Default", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetDefaultConfig());
                }

                if (GUILayout.Button("Beach", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetBeachWorldConfig());
                }

                if (GUILayout.Button("Mountains", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetMountainousWorldConfig());
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Archipelago", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetArchipelagoConfig());
                }

                if (GUILayout.Button("Earth-like", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetEarthLikeConfig());
                }

                if (GUILayout.Button("Flat", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetFlatWorldConfig());
                }
            }

            
            

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Jungle", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetJungleWorldConfig());
                }

                if (GUILayout.Button("Cavernous", GUILayout.Height(25)))
                {
                    ApplyPreset(WorldGenPresets.GetCavernousWorldConfig());
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Regenerate", GUILayout.Height(30)))
            {
                _visualizer.RegenerateHeightMap();
            }
        }

        private void ApplyPreset(BurstSphericalNoiseConfig config)
        {
            SerializedObject serialized = new SerializedObject(_visualizer);

            SetField(serialized, "terrainHeight", config.TerrainHeight);
            SetField(serialized, "continentOctaves", config.ContinentOctaves);
            SetField(serialized, "continentPersistence", config.ContinentPersistence);
            SetField(serialized, "continentFrequency", config.ContinentFrequency);
            SetField(serialized, "mountainMaskFrequency", config.MountainMaskFrequency);
            SetField(serialized, "mountainThreshold", config.MountainThreshold);
            SetField(serialized, "mountainBlend", config.MountainBlend);
            SetField(serialized, "mountainFrequency", config.MountainFrequency);
            SetField(serialized, "mountainOctaves", config.MountainOctaves);
            SetField(serialized, "mountainPersistence", config.MountainPersistence);
            SetField(serialized, "mountainSharpness", config.MountainSharpness);
            SetField(serialized, "plainsStrength", config.PlainsStrength);
            SetField(serialized, "plainsFrequency", config.PlainsFrequency);
            SetField(serialized, "detailFrequency", config.DetailFrequency);
            SetField(serialized, "detailStrength", config.DetailStrength);
            SetField(serialized, "warpFrequency", config.WarpFrequency);
            SetField(serialized, "warpStrength", config.WarpStrength);

            serialized.ApplyModifiedProperties();

            _visualizer.RegenerateHeightMap();
            EditorUtility.SetDirty(_visualizer);
        }

        private void SetField(SerializedObject serialized, string fieldName, float value)
        {
            SerializedProperty prop = serialized.FindProperty(fieldName);
            if (prop != null && prop.propertyType == SerializedPropertyType.Float)
            {
                prop.floatValue = value;
            }
        }

        private void SetField(SerializedObject serialized, string fieldName, int value)
        {
            SerializedProperty prop = serialized.FindProperty(fieldName);
            if (prop != null && prop.propertyType == SerializedPropertyType.Integer)
            {
                prop.intValue = value;
            }
        }
    }
}
#endif


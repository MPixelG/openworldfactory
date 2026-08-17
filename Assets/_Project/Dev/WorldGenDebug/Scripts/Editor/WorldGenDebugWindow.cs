#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _Project.Dev.WorldGenDebug.Scripts.Editor
{
    /// <summary>
    /// Editor window for previewing and debugging world generation.
    /// Displays the height map and allows real-time parameter tweaking.
    /// </summary>
    public class WorldGenDebugWindow : EditorWindow
    {
        private WorldGenDebugVisualizer _visualizer;
        private Vector2 _scrollPosition;
        private Texture2D _previewTexture;

        [MenuItem("Window/World Gen Debug")]
        public static void ShowWindow()
        {
            GetWindow<WorldGenDebugWindow>("World Gen Debug");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("World Generation Debugger", EditorStyles.boldLabel);

            // Visualizer selection
            WorldGenDebugVisualizer newVisualizer = EditorGUILayout.ObjectField(
                "Visualizer",
                _visualizer,
                typeof(WorldGenDebugVisualizer),
                true
            ) as WorldGenDebugVisualizer;

            _visualizer = newVisualizer;

            if (_visualizer == null)
            {
                EditorGUILayout.HelpBox(
                    "No WorldGenDebugVisualizer selected. Select one from the scene or create a new gameobject with the WorldGenDebugVisualizer component.",
                    MessageType.Info
                );
                return;
            }

            EditorGUILayout.Space();

            // Preview texture display
            EditorGUILayout.LabelField("Height Map Preview", EditorStyles.boldLabel);

            Texture2D heightMap = _visualizer.GetHeightMapTexture();
            if (heightMap != null)
            {
                float previewSize = Mathf.Min(position.width - 20, 512);
                EditorGUILayout.LabelField("", GUILayout.Height(previewSize));
                Rect previewRect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(previewRect, heightMap, ScaleMode.ScaleToFit);

                EditorGUILayout.LabelField(
                    $"Texture Resolution: {heightMap.width}x{heightMap.height}",
                    EditorStyles.miniLabel
                );
            }
            else
            {
                EditorGUILayout.HelpBox("Height map not generated yet. Click 'Regenerate' to create it.", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Controls
            if (GUILayout.Button("Regenerate Height Map", GUILayout.Height(30)))
            {
                _visualizer.RegenerateHeightMap();
            }

            if (GUILayout.Button("Export Height Map as PNG"))
            {
                ExportHeightMap();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Select Visualizer in Scene"))
            {
                EditorGUIUtility.PingObject(_visualizer);
                Selection.activeGameObject = _visualizer.gameObject;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);

            // Display information about the current configuration
            if (heightMap != null)
            {
                EditorGUILayout.LabelField($"Texture Memory: ~{heightMap.width * heightMap.height * 4 / 1024}KB", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space();

            // Help text
            EditorGUILayout.HelpBox(
                "To use this debugger:\n" +
                "1. Create a new GameObject in your scene\n" +
                "2. Add the 'WorldGenDebugVisualizer' component\n" +
                "3. Assign a MaterialDatabase\n" +
                "4. Tweak the noise parameters in the inspector\n" +
                "5. The height map will update in real-time\n" +
                "6. Use this window to preview and export the result",
                MessageType.Info
            );
        }

        private void ExportHeightMap()
        {
            if (_visualizer == null)
            {
                EditorUtility.DisplayDialog("Error", "No visualizer selected.", "OK");
                return;
            }

            Texture2D heightMap = _visualizer.GetHeightMapTexture();
            if (heightMap == null)
            {
                EditorUtility.DisplayDialog("Error", "No height map generated yet.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel(
                "Export Height Map",
                "",
                "heightmap.png",
                "png"
            );

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            byte[] bytes = heightMap.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            EditorUtility.DisplayDialog("Success", $"Height map exported to:\n{path}", "OK");
        }
    }
}
#endif


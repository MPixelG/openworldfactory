using UnityEditor;
using UnityEngine;

namespace _Project.World.Atmosphere.Shader
{
    
    public class RenderTextureDebugger : EditorWindow
    {
        private RenderTexture targetTexture;

        [MenuItem("Window/Analysis/Render Texture Debugger")]
        public static void ShowWindow()
        {
            GetWindow<RenderTextureDebugger>("Texture Debugger");
        }

        void OnGUI()
        {
            // Field to assign the render texture
            targetTexture = (RenderTexture)EditorGUILayout.ObjectField("Target Texture", targetTexture, typeof(RenderTexture), true);

            if (targetTexture != null)
            {
                // Draw the texture to fill the remaining window space
                Rect rect = GUILayoutUtility.GetRect(position.width, position.height - 30);
                EditorGUI.DrawPreviewTexture(rect, targetTexture);
            }
        }

        // Forces the window to repaint every frame during playback so it updates smoothly
        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
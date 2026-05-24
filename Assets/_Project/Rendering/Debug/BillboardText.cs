using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Project.Rendering.Debug
{
    [ExecuteAlways]
    public class BillboardText : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera cam = null;

#if UNITY_EDITOR
            if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
            {
                cam = SceneView.lastActiveSceneView.camera;
            }
#endif

            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam == null) return;

            transform.rotation = cam.transform.rotation;
        }
    }
}
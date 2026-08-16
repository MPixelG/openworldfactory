using UnityEngine;

#if UNITY_EDITOR
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using UnityEditor;
#endif

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    [CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Planet/Material Database")]
    public class MaterialDatabase : ScriptableObject
    {
        [SerializeField]
        private Material[] materials;

        public Material GetMaterial(VoxelMaterial type)
        {
            return materials[(int)type];
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(MaterialDatabase))]
    public class MaterialDatabaseEditor : Editor
    {
        private SerializedProperty _materials;

        private void OnEnable()
        {
            _materials = serializedObject.FindProperty("materials");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(
                "Materials",
                EditorStyles.boldLabel
            );

            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length;

            if (_materials.arraySize != materialCount)
                _materials.arraySize = materialCount;

            for (int i = 0; i < materialCount; i++)
            {
                VoxelMaterial type = (VoxelMaterial)i;

                SerializedProperty element =
                    _materials.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(
                    element,
                    new GUIContent(type.ToString())
                );
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}





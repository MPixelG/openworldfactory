using UnityEngine;

namespace _Project.World.Planet.Scripts
{
// Das Attribut muss vor der Klassendeklaration stehen
    [ExecuteAlways] // läuft auch im Editor
    public class PlanetCube : MonoBehaviour
    {
        //tracks when changes are happening
        private bool _needsGeneration;

        // Controls how detailed each cube face mesh is
        [Range(2, 256)] public int resolution = 10;

        [SerializeField, HideInInspector] private MeshFilter[] meshFilters;

        // One TerrainFace per cube side (6 total)
        private TerrainFace[] _terrainFaces;

        private void Start()
        {
            Initialize();
            GenerateMesh();
        }

        // Called automatically in editor when values change
        private void OnValidate()
        {
            // Just set the flag, do NO structural work here
            if (gameObject.activeInHierarchy)
            {
                _needsGeneration = true;
            }
        }

        private void Update()
        {
            // update is safe for structural change to meshes etc
            if (_needsGeneration)
            {
                Initialize();
                GenerateMesh();

                // Reset the flag so it only runs once per change
                _needsGeneration = false;
            }
        }

        private void Initialize()
        {
            // Ensure we always have 6 mesh slots (cube faces)
            if (meshFilters == null || meshFilters.Length != 6)
            {
                meshFilters = new MeshFilter[6];
            }

            _terrainFaces = new TerrainFace[6];

            // Directions for each cube face
            Vector3[] directions =
            {
                Vector3.up, Vector3.down,
                Vector3.left, Vector3.right,
                Vector3.forward, Vector3.back
            };

            for (int i = 0; i < 6; i++)
            {
                // Create mesh objects if they don't exist yet
                if (meshFilters[i] == null)
                {
                    // Try to find existing children first
                    Transform child = transform.Find("mesh_" + i);
                    if (child != null)
                    {
                        meshFilters[i] = child.GetComponent<MeshFilter>();
                    }
                    else
                    {
                        GameObject meshObj = new GameObject("mesh_" + i);
                        // Parent meshes to the planet object
                        meshObj.transform.parent = transform;
                        // Add renderer + material
                        meshObj.AddComponent<MeshRenderer>()
                            .sharedMaterial = new Material(Shader.Find("Standard"));
                        // Add mesh filter with empty mesh
                        meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                        meshFilters[i].sharedMesh = new Mesh();
                    }
                }

                // Create a terrain face pointing in one cube direction
                _terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i]);
            }
        }

        // Builds all 6 cube faces
        private void GenerateMesh()
        {
            if (_terrainFaces == null) return;
            foreach (TerrainFace face in _terrainFaces)
            {
                face.ConstructMesh();
            }
        }
    }
}
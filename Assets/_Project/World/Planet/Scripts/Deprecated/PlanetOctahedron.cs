
using UnityEngine;

namespace _Project.World.Planet.Scripts.Deprecated
{
    [ExecuteAlways] //it should also run in editor mode otherwise prefab collaboration could be a problem
    public class Planet : MonoBehaviour
    {
        //changes flag
        private bool _needsGeneration;
        // Controls mesh detail level
        [Range(1,256)]
        public int resolution = 10;
    
        [SerializeField, HideInInspector]
        private MeshFilter[] meshFilters;
        // Stores all terrain faces of the planet
        private TerrainFace[]  _terrainFaces;
    
        // Automatically rebuilds the planet
        // whenever values change in the Inspector
        // Called automatically in editor when values change
        private void OnValidate()
        {
            //Just set the flag, do NO structural work here
            if (gameObject.activeInHierarchy)
            {
                _needsGeneration = true;
            }
        }

        private void Update()
        {
            //update is safe for structural change to meshes etc
            if (_needsGeneration)
            {
                Initialize();
                GenerateMesh();
            
                // Reset the flag so it only runs once per change
                _needsGeneration = false; 
            }
        }

        void Initialize()
        {
      
            // Ensure we have 8 mesh filters
            // (one for each octahedron face)
            if (meshFilters == null || meshFilters.Length != 8)
            {
                meshFilters = new MeshFilter[8];
            }
            _terrainFaces = new TerrainFace[8];
            // Vertices for an octahedron
            // Every 3 vertices define one triangle face
            Vector3[] vertices =
            {
                Vector3.up,Vector3.right,Vector3.forward,
                Vector3.up,Vector3.back,Vector3.right,
                Vector3.up,Vector3.left,Vector3.back,
                Vector3.up,Vector3.forward,Vector3.left,
                Vector3.forward,Vector3.right,Vector3.down,
                Vector3.right,Vector3.back,Vector3.down,
                Vector3.back,Vector3.left,Vector3.down,
                Vector3.left,Vector3.forward,Vector3.down
            };
        
            // Create all 8 triangle faces
            for (int i = 0; i < 8; i++)
            {   //name the faces for unity to know which face is generated and referenced
                string faceName = $"OctaFace_{i}";
                // reconnect to reference 
                if (meshFilters[i] == null)
                {
                    Transform child = transform.Find(faceName);
                    if (child != null)
                    {
                        meshFilters[i] = child.GetComponent<MeshFilter>();
                    }
                }
                //if they still does not exist meaning there is no reference
                if (meshFilters[i] == null)
                {
                    GameObject meshObj = new GameObject(faceName)
                    {
                        transform =
                        {
                            // Parent meshes to the planet object
                            parent = transform
                        }
                    };
                    // Add renderer + material
                    meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                    // Add mesh filter with empty mesh
                    meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                    meshFilters[i].sharedMesh = new Mesh();
                }
                // Get the 3 vertices for this triangle face skipped triCount for i variable
                Vector3[] vert = {vertices[i*3],vertices[i*3+1],vertices[i*3+2]};
                // Create terrain face for this triangle
                _terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, vert);
            }
        }
    
        // Generates the mesh for every face
        private void GenerateMesh()
        {
            foreach (TerrainFace face in _terrainFaces)
            {
                face.ConstructMesh();
            } 
        }
    }
}

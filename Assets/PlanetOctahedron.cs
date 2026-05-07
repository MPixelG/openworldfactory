
using UnityEngine;

public class Planet : MonoBehaviour
{
    // Controls mesh detail level
    [Range(1,256)]
    public int resolution = 10;
    [SerializeField, HideInInspector]
    
    private MeshFilter[] meshFilters;
    // Stores all terrain faces of the planet
    private TerrainFace[]  _terrainFaces;
    
    // Automatically rebuilds the planet
    // whenever values change in the Inspector
    private void OnValidate()
    {
        Initialize();
        GenerateMesh();
    }

    void Initialize()
    {
        int triCount = 0;
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
        {
            // Create mesh objects if they don't exist yet
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                // Parent meshes to the planet object
                meshObj.transform.parent = transform;
                // Add renderer + material
                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                // Add mesh filter with empty mesh
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            // Get the 3 vertices for this triangle face
            Vector3[] vert = {vertices[triCount],vertices[triCount+1],vertices[triCount+2]};
            triCount += 3;
            // Create terrain face for this triangle
            _terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, vert);
        }
    }
    
    // Generates the mesh for every face
    void GenerateMesh()
    {
        foreach (TerrainFace face in _terrainFaces)
        {
            face.ConstructMesh();
        } 
    }
}

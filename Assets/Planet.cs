
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(1,256)]
    public int resolution = 10;
    [SerializeField, HideInInspector]
    //
    private MeshFilter[] meshFilters;
    private TerrainFace[]  _terrainFaces;

    private void OnValidate()
    {
        Initialize();
        GenerateMesh();
    }

    void Initialize()
    {
        int triCount = 0;
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[8];
        }
        _terrainFaces = new TerrainFace[8];
        
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
        
        for (int i = 0; i < 8; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;
                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            Vector3[] vert = {vertices[triCount],vertices[triCount+1],vertices[triCount+2]};
            triCount += 3;
            _terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, vert);
        }
    }

    void GenerateMesh()
    {
        foreach (TerrainFace face in _terrainFaces)
        {
            face.ConstructMesh();
        } 
    }
}

using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking

{
    /// <summary>
    /// A chunk is a region of density values in space. it is used to split the terrain into multiple portions and put these chunks next to each other to allow dynamic loading / unloading of specific regions, different LODs (Levels of Details) and more.  
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        private static readonly int Cull = Shader.PropertyToID("_Cull");

        private MeshFilter _meshFilter; // the mesh filter is used to pass the vertices to unity.
        private MeshRenderer _meshRenderer; // and the mesh renderer actually renders these. 

        private void Awake()
        {
            EnsureComponents();
        }
        
        //ensures the components (currently the mesh filter and renderer) are applied to the game object
        private void EnsureComponents()
        {
            if (_meshFilter == null)
                _meshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            if (_meshRenderer == null)
                _meshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
        }
        
        private void SetupMaterial()
        {
            if (_meshRenderer.sharedMaterial != null) return; // if a material is already set we dont need to set one ourselves

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") // use the default lit shader
                            ?? Shader.Find("Standard"); // or the standard one as a backup

            var mat = new Material(shader); // apply that shader to a new material

            _meshRenderer.sharedMaterial = mat; // and apply that material to the renderer
        }


        /// <summary>
        /// applies a given mesh to this renderer
        /// </summary>
        /// <param name="mesh"></param>
        public void ApplyMeshData(Mesh mesh)
        {
            EnsureComponents(); // ensure the renderer and filter are available
            SetupMaterial(); // and also ensure a material is set for the renderer so that the mesh is actually visible
            _meshFilter.mesh = mesh; // finally apply the mesh to the filter for it to get rendered
        }
        
    }
}
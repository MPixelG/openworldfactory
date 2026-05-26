using System;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking

{
    /// <summary>
    /// A chunk is a region of density values in space. it is used to split the terrain into multiple portions and put these chunks next to each other to allow dynamic loading / unloading of specific regions, different LODs (Levels of Details) and more.  
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        
        
        public ChunkData Data;
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
            if (_meshRenderer.sharedMaterial != null) return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") 
                            ?? Shader.Find("Standard");

            var mat = new Material(shader);
            mat.SetInt(Cull, (int)UnityEngine.Rendering.CullMode.Back);

            _meshRenderer.sharedMaterial = mat;
        }


        public void ApplyMeshData(Mesh mesh)
        {
            EnsureComponents();
            SetupMaterial();
            _meshFilter.mesh = mesh;
        }
        
    }
}
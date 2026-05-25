using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking

{
    /// <summary>
    /// A chunk is a region of density values in space. it is used to split the terrain into multiple portions and put these chunks next to each other to allow dynamic loading / unloading of specific regions, different LODs (Levels of Details) and more.  
    /// </summary>
    public abstract class Chunk : MonoBehaviour
    {
        protected MeshFilter MeshFilter; // the mesh filter is used to pass the vertices to unity.
        protected MeshRenderer MeshRenderer; // and the mesh renderer actually renders these. 
    }
}
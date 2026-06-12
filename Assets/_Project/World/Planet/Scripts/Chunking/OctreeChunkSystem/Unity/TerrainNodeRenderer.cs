using System.Collections.Generic;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class TerrainNodeRenderer : MonoBehaviour
    {
        [SerializeField] private bool drawOctree = true;

        private Dictionary<int, ChunkRenderer> _chunkRenderers = new();
        
        
        
        private void OnDrawGizmos()
        {
            if(!drawOctree)
                return;

            //DrawNode(root);
        }
    }
}
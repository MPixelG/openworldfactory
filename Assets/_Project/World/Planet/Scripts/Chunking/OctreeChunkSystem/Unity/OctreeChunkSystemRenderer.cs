using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class OctreeChunkSystemRenderer : MonoBehaviour
    {
        private OctreeChunkManager _chunkManager;
        private Transform _viewer;
        
        [SerializeField, Range(0, 10)] private byte octreeGizmoDrawLayer = 0; 
        
        public void SetChunkManager(OctreeChunkManager chunkManager)
        {
            _chunkManager = chunkManager;
        }

        public void OnDrawGizmos()
        {
            if(!_chunkManager.OctreeReady) return;
            Octree octree = _chunkManager.Octree;
            
            
            float3 rootSize = octree.Max - octree.Min;
            
            foreach (OctreeNode octreeNode in _chunkManager.Octree.Nodes)
            {
                if (octreeNode.Depth > octreeGizmoDrawLayer) continue;
                Gizmos.color = Color.Lerp(Color.white, Color.red, (float) octreeNode.Depth / octreeGizmoDrawLayer).WithAlpha(0.1f);
                int3 min = octreeNode.Coord;
                float3 size = rootSize / (1 << octreeNode.Depth);
                Gizmos.DrawWireCube(min + size/2, size);
                
                
                if (octreeNode.Depth != octreeGizmoDrawLayer) continue;
                Gizmos.color = (octreeNode.State)switch
                {
                    OctreeNodeState.Empty => Color.purple,
                    OctreeNodeState.Full => Color.green.WithAlpha(0.1f),
                    OctreeNodeState.Mixed => Color.yellow.WithAlpha(0.05f),
                    OctreeNodeState.Unknown => Color.blue,
                    _ => Gizmos.color
                };

                Gizmos.DrawCube(min + size / 2, size/5);
            }
        }
        

        private void Update() //todo use coroutine + timer
        {
            float3 viewerPosition = _viewer != null ? _viewer.position : float3.zero;
            _chunkManager?.Update(viewerPosition);
        }
    }
}
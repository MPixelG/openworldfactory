using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class OctreeChunkSystemRenderer : MonoBehaviour
    {
        private OctreeChunkManager _chunkManager;
        
        private Transform _viewer;
        
        private readonly Queue<NodeChange> _chunkChanges = new();
        
        [SerializeField, Range(0, 10)] private byte octreeGizmoDrawLayer = 0; 
        
        public void SetChunkManager(OctreeChunkManager chunkManager)
        {
            // unsubscribe old
            if (_chunkManager != null)
                _chunkManager.NodeChange -= OnChunkChange;
            
            _chunkManager = chunkManager;
            
            if (_chunkManager != null)
                _chunkManager.NodeChange += OnChunkChange;
        }

        private void OnChunkChange(NodeChange chunkChange)
        {
            _chunkChanges.Enqueue(chunkChange);
        }

        public void OnDrawGizmos()
        {
            Octree octree = _chunkManager.Octree;
            
            
            float3 rootSize = octree.Max - octree.Min;
            
            foreach (OctreeNode octreeNode in _chunkManager.Octree.Nodes)
            {
                if (octreeNode.Depth > octreeGizmoDrawLayer) continue;
                Gizmos.color = Color.Lerp(Color.white, Color.red, (float) octreeNode.Depth / octreeGizmoDrawLayer);
                int3 min = octreeNode.Coord;
                float3 size = rootSize / (1 << octreeNode.Depth);
                Gizmos.DrawWireCube(min + size/2, size);
                
                
                if (octreeNode.Depth != octreeGizmoDrawLayer) continue;
                switch (octreeNode.State)
                {
                    case OctreeNodeState.Empty: Gizmos.color = Color.purple.WithAlpha(0.2f); break;
                    case OctreeNodeState.Full: Gizmos.color = Color.green; break;
                    case OctreeNodeState.Mixed: Gizmos.color = Color.yellow.WithAlpha(0.6f); break;
                    case OctreeNodeState.Unknown: Gizmos.color = Color.blue; break;
                }
                
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
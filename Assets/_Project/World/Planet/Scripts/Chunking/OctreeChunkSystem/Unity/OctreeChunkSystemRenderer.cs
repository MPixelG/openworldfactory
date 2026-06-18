using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class OctreeChunkSystemRenderer : MonoBehaviour
    {
        private OctreeChunkManager _chunkManager;
        private Transform _viewer;
        
        [Header("Debug Settings")]
        [SerializeField, Range(0, 10)] private byte octreeGizmoDrawLayer; 
        [SerializeField] private bool drawAllLayersUpToSelected;
        [SerializeField] private bool showSolidStateCubes = true;
        
        public void SetChunkManager(OctreeChunkManager chunkManager)
        {
            _chunkManager = chunkManager;
        }

        public void OnDrawGizmos()
        {
            if (_chunkManager == null || !_chunkManager.OctreeReady) return;
            
            Octree octree = _chunkManager.Octree;
            byte maxDepth = _chunkManager.Octree.MaxDepth;

            foreach (OctreeNode octreeNode in octree.Nodes)
            {
                byte depth = octreeNode.MortonCode.GetDepth();
                
                if (drawAllLayersUpToSelected)
                {
                    if (depth > octreeGizmoDrawLayer) continue;
                }
                else
                {
                    if (depth != octreeGizmoDrawLayer) continue;
                }

                int nodeSize = 1 << (maxDepth - depth);
                float3 size = new float3(nodeSize);

                int3 localGridPos = octreeNode.MortonCode.DecodeCoord();
                float3 worldMin = octree.Min + (localGridPos * nodeSize);
                float3 center = worldMin + (size * 0.5f);
                
                float depthLerp = maxDepth > 0 ? (float)depth / maxDepth : 0f;
                Color wireColor = Color.Lerp(Color.white, Color.red, depthLerp);
                wireColor.a = depth == octreeGizmoDrawLayer ? 0.15f : 0.05f;
                Gizmos.color = wireColor;
                
                Gizmos.DrawWireCube(center, size);
                
                if (showSolidStateCubes)
                {
                    Color gizmoColor = octreeNode.State switch
                    {
                        OctreeNodeState.Empty => new Color(0.5f, 0f, 0.5f, 0.2f),
                        OctreeNodeState.Full => new Color(0f, 1f, 0f, 0.1f),
                        OctreeNodeState.Mixed => new Color(1f, 0.92f, 0.016f, 0.4f),
                        OctreeNodeState.Unknown => Color.blue,
                        _ => Gizmos.color
                    };
                    
                    if(depth != octreeGizmoDrawLayer) gizmoColor = gizmoColor.WithAlphaMultiplied(0.3f);
                    Gizmos.color = gizmoColor;

                    float cubeScale = octreeNode.State == OctreeNodeState.Mixed ? 0.25f : 0.15f;
                    Gizmos.DrawCube(center, size * cubeScale);
                }
            }
        }

        private void Update()
        {
            float3 viewerPosition = _viewer != null ? _viewer.position : float3.zero;
            _chunkManager?.Update(viewerPosition);
        }
    }
}
using System;
using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.v2.Data;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2.Rendering
{
    public class PlanetRenderer : MonoBehaviour
    {
        private PlanetManager _planetManager;
        
        private FrustumCullingSystem _frustumCullingSystem;
        
        [SerializeField] private Camera viewer;
        [SerializeField, Range(0, 20)] private ushort minDrawDepth;
        [SerializeField, Range(0, 20)] private ushort maxDrawDepth=20;
        [SerializeField] private bool drawGizmosPoints = true;
        [SerializeField] private bool drawGizmosOutlines = true;
        [SerializeField] private bool drawMesh = true;


        private Dictionary<ulong, Mesh> _chunkMeshes = new();
        
        
        public void SetPlanetManager(PlanetManager planetManager)
        {
            if(_planetManager != null) _planetManager.ChunkChange -= OnChunkChange;
            
            _planetManager = planetManager;
            _planetManager.ChunkChange += OnChunkChange;
            Debug.Log("PLANET MANAGER SET");
        }


        private void OnChunkChange(ChunkChange change)
        {
            switch (change.ChangeType)
            {
                case ChunkChangeType.Update:
                case ChunkChangeType.Load:
                {
                    if (change.Payload is {} payload)
                    {
                        Mesh mesh = UnityMeshBuilder.Build(
                            payload.Vertices,
                            payload.Normals,
                            payload.Indices
                        );
                        _chunkMeshes[change.MortonCode] = mesh;
                    } else Debug.LogWarning($"Payload is null for MortonCode: {change.MortonCode}");
                    
                    break;
                }
                case ChunkChangeType.Unload:
                    _chunkMeshes.Remove(change.MortonCode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnEnable()
        {
            _frustumCullingSystem = new FrustumCullingSystem();
            _chunkMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        private void Update()
        {
            if(_frustumCullingSystem == null) Debug.LogWarning("FrustumCullingSystem is null");
            if(_planetManager == null) Debug.LogWarning("Planet Manager is null");
            if(viewer == null) Debug.LogWarning("Viewer is null");
            if(_frustumCullingSystem == null || _planetManager == null || viewer == null) return;
            if (_planetManager.OctreeReady)
            {
                _frustumCullingSystem?.Update(
                    _planetManager.Octree,
                    viewer
                );
            }
            

            _planetManager.Update();
            
            if(drawMesh) DrawChunks();
        }

        private Material _chunkMaterial;
        
        private void DrawChunks()
        {
            foreach (ulong chunkMeshesCoord in _chunkMeshes.Keys)
            {
                byte depth = chunkMeshesCoord.GetDepth();
                if (depth < minDrawDepth || depth > maxDrawDepth) continue;

                int nodeSize =
                    1 << (_planetManager.Octree.MaxDepth - depth);

                float3 pos =
                    _planetManager.Octree.Min +
                    chunkMeshesCoord.DecodeToCoord() * nodeSize;
                
                Graphics.DrawMesh(
                    _chunkMeshes[chunkMeshesCoord],
                    new Vector3(pos.x, pos.y, pos.z),
                    Quaternion.identity,
                    _chunkMaterial,
                    0
                );
                
            }
        }

        private void OnDrawGizmos()
        {
            if(_planetManager == null || (!drawGizmosPoints && !drawGizmosOutlines))
                return;
            
            foreach (ulong chunkMeshesCoord in _chunkMeshes.Keys)
            {
                byte depth = chunkMeshesCoord.GetDepth();
                
                if (depth < minDrawDepth || depth > maxDrawDepth) continue;

                int nodeSize =
                    1 << (_planetManager.Octree.MaxDepth - depth);

                /*float3 pos =
                    _planetManager.Octree.Min +
                    chunkMeshesCoord.DecodeToCoord() * nodeSize;
                Gizmos.DrawWireCube(new Vector3(pos.x + (nodeSize / 2f), pos.y + (nodeSize / 2f), pos.z +
                    (nodeSize / 2f)), new Vector3(nodeSize, nodeSize, nodeSize));*/
                if (drawGizmosPoints)
                {
                    int3 c = chunkMeshesCoord.DecodeToCoord();
                    float3 p = _planetManager.Octree.Min + c * nodeSize;
                    OctreeNode? node = _planetManager.Octree.GetNodeAtPosition(chunkMeshesCoord);
                    if(node == null) continue;
                    OctreeNodeState state = node.Value.State;
                    Color gizmosColor = state switch
                    {
                        OctreeNodeState.Empty => Color.red,
                        OctreeNodeState.Full => Color.green,
                        OctreeNodeState.Mixed => Color.yellow,
                        OctreeNodeState.Unknown => Color.blue,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    Gizmos.color = gizmosColor;
                    Gizmos.DrawSphere(p, 1);
                }

                if (drawGizmosOutlines)
                {
                    Bounds b = _planetManager.Octree.GetBounds(chunkMeshesCoord);
                    Gizmos.DrawWireCube(b.center, b.size);
                }
            }
        }

        private void OnDestroy()
        {
            _planetManager.ChunkChange -= OnChunkChange;
            _planetManager.Dispose();
        }
    }
}
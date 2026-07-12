using System;
using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.v2.Data;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2.Rendering
{
    public class PlanetRenderer : MonoBehaviour
    {
        private PlanetManager _planetManager;
        
        private FrustumCullingSystem _frustumCullingSystem;
        
        [SerializeField] private Camera viewer;
        [SerializeField] private bool drawGizmos = true;


        private Dictionary<ulong, Mesh> _chunkMeshes = new();
        
        
        public void SetPlanetManager(PlanetManager planetManager)
        {
            if(_planetManager != null) _planetManager.ChunkChange -= OnChunkChange;
            
            _planetManager = planetManager;
            _planetManager.ChunkChange += OnChunkChange;
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
            _frustumCullingSystem?.Update(
                _planetManager.Octree,
                viewer
            );

            _planetManager.Update();
            
            DrawChunks();
        }

        private Material _chunkMaterial;
        
        private void DrawChunks()
        {
            foreach (ulong chunkMeshesCoord in _chunkMeshes.Keys)
            {
                byte depth = chunkMeshesCoord.GetDepth();

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
            if(_planetManager == null || !drawGizmos)
                return;
            
            foreach (ulong chunkMeshesCoord in _chunkMeshes.Keys)
            {
                byte depth = chunkMeshesCoord.GetDepth();

                int nodeSize =
                    1 << (_planetManager.Octree.MaxDepth - depth);

                /*float3 pos =
                    _planetManager.Octree.Min +
                    chunkMeshesCoord.DecodeToCoord() * nodeSize;
                Gizmos.DrawWireCube(new Vector3(pos.x + (nodeSize / 2f), pos.y + (nodeSize / 2f), pos.z +
                    (nodeSize / 2f)), new Vector3(nodeSize, nodeSize, nodeSize));*/
                
                int3 c = chunkMeshesCoord.DecodeToCoord();

                float3 p = _planetManager.Octree.Min + c*nodeSize;

                Gizmos.DrawSphere(p,1);


                Bounds b = _planetManager.Octree.GetBounds(chunkMeshesCoord);
                Gizmos.DrawWireCube(b.center,b.size);
            }
        }

        private void OnDestroy()
        {
            _planetManager.ChunkChange -= OnChunkChange;
            _planetManager.Dispose();
        }
    }
}
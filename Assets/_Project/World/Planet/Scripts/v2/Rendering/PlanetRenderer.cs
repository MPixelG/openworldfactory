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
        
        private FrustumCullingSystem _frustumCullingSystem = new();
        
        [SerializeField] private Camera viewer;


        private Dictionary<ulong, Mesh> _chunkMeshes = new();
        
        
        public void SetPlanetManager(PlanetManager planetManager)
        {
            if(_planetManager != null) _planetManager.ChunkChange -= OnChunkChange;
            
            _planetManager = planetManager;
            _planetManager.ChunkChange += OnChunkChange;
        }


        private void OnChunkChange(ChunkChange change)
        {
            Mesh mesh = UnityMeshBuilder.Build(
                change.Payload.Vertices,
                change.Payload.Normals,
                change.Payload.Indices
            );
            
            _chunkMeshes[change.MortonCode] = mesh;
        }

        private void OnEnable()
        {
            _chunkMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        private void Update()
        {
            _frustumCullingSystem.Update(
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
            if(_planetManager == null)
                return;
            
            foreach (ulong chunkMeshesCoord in _chunkMeshes.Keys)
            {
                byte depth = chunkMeshesCoord.GetDepth();

                int nodeSize =
                    1 << (_planetManager.Octree.MaxDepth - depth);

                float3 pos =
                    _planetManager.Octree.Min +
                    chunkMeshesCoord.DecodeToCoord() * nodeSize;
                Gizmos.DrawWireCube(new Vector3(pos.x + (nodeSize / 2f), pos.y + (nodeSize / 2f), pos.z +
                    (nodeSize / 2f)), new Vector3(nodeSize, nodeSize, nodeSize));
            }
        }

        private void OnDestroy()
        {
            _planetManager.ChunkChange -= OnChunkChange;
            _planetManager.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Data;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Rendering
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

        private MaterialDatabase _materialDatabase;


        private readonly Dictionary<ulong, Mesh> _chunkMeshes = new();
        
        
        public void SetPlanetManager(PlanetManager planetManager)
        {
            if(_planetManager != null) _planetManager.ChunkChange -= OnChunkChange;
            
            _planetManager = planetManager;
            _planetManager.ChunkChange += OnChunkChange;
        }

        public void SetMaterialDatabase(MaterialDatabase materialDatabase)
        {
            _materialDatabase = materialDatabase;
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
                            payload.Triangles
                        );
                        _chunkMeshes[change.MortonCode] = mesh;
                        payload.Dispose(); //TODO fix, may cause issues when there are multiple listeners that use the same allocation, leads to race condition 
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
            

            _planetManager.Update();
            
            if(drawMesh) DrawChunks();
        }

        private Material _chunkMaterial;
        
        private void DrawChunks()
        {
            int maxDepth = _planetManager.Octree.MaxDepth;
            float3 min = _planetManager.Octree.Min;
            float3 max = _planetManager.Octree.Max;
            
            int maxOctreeSize = 1 << maxDepth;
            
            foreach (ulong chunkMeshesCoord in _chunkMeshes.Keys)
            {
                byte depth = chunkMeshesCoord.GetDepth();
                if (depth < minDrawDepth || depth > maxDrawDepth) continue;

                int logicalNodeSize = 1 << (maxDepth - depth);
                float3 nodeSize = (max - min)*((float)logicalNodeSize/maxOctreeSize);

                float3 localPos = chunkMeshesCoord.DecodeToCoord();
                float3 worldPos = min + localPos * nodeSize;
                
                Mesh mesh = _chunkMeshes[chunkMeshesCoord];
                if (mesh.vertexCount == 0) continue;
                
                float3 size = max - min;

                
                int submeshCount = _chunkMeshes[chunkMeshesCoord].subMeshCount;
                for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                {
                    Material material = _materialDatabase.GetMaterial((VoxelMaterial) submeshIndex);
                    if (material == null)
                    {
                        continue;
                    }
                    var renderParams = new RenderParams(material)
                    { 
                        worldBounds = new Bounds(min + size*0.5f, size)
                    };
                    
                    Graphics.RenderMesh(
                        renderParams,
                        _chunkMeshes[chunkMeshesCoord],
                        submeshIndex,
                        Matrix4x4.Translate(new Vector3(worldPos.x, worldPos.y, worldPos.z))
                    );
                }
            }
        }

        private void OnDrawGizmos()
        { 
            if(_planetManager == null || (!drawGizmosPoints && !drawGizmosOutlines))
                return;
            
            int maxDepth = _planetManager.Octree.MaxDepth;
            float3 min = _planetManager.Octree.Min;
            float3 max = _planetManager.Octree.Max;
            
            int maxOctreeSize = 1 << maxDepth;
            
            foreach (ulong chunkMeshesCoord in _chunkMeshes.Keys)
            {
                byte depth = chunkMeshesCoord.GetDepth();
                if (depth < minDrawDepth || depth > maxDrawDepth) continue;
                
                int logicalNodeSize = 1 << (maxDepth - depth);
                float3 nodeSize = (max - min)*((float)logicalNodeSize/maxOctreeSize);

                float3 localPos = chunkMeshesCoord.DecodeToCoord();
                float3 worldPos = min + localPos * nodeSize;

                /*float3 pos =
                    _planetManager.Octree.Min +
                    chunkMeshesCoord.DecodeToCoord() * nodeSize;
                Gizmos.DrawWireCube(new Vector3(pos.x + (nodeSize / 2f), pos.y + (nodeSize / 2f), pos.z +
                    (nodeSize / 2f)), new Vector3(nodeSize, nodeSize, nodeSize));*/
                if (drawGizmosPoints)
                {
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
                    Gizmos.DrawSphere(worldPos, 1);
                }

                if (drawGizmosOutlines)
                {
                    Gizmos.DrawWireCube(worldPos + new float3(nodeSize / 2f),new float3(nodeSize));
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
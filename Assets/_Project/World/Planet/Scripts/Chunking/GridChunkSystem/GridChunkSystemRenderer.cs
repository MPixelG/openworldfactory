using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    public class GridChunkSystemRenderer : MonoBehaviour
    {
        private GridChunkManager _chunkManager;
        private Transform _viewer;

        private readonly Dictionary<ChunkCoord, ChunkRenderer> _chunkRenderers = new();
        private readonly Queue<ChunkChange> _chunkChanges = new();

        public void SetChunkManager(GridChunkManager chunkManager)
        {
            // unsubscribe old
            if (_chunkManager != null)
                _chunkManager.ChunkChange -= OnChunkChange;
            
            _chunkManager = chunkManager;
            
            if (_chunkManager != null)
                _chunkManager.ChunkChange += OnChunkChange;
        }

        public void SetViewer(Transform viewer)
        {
            _viewer = viewer;
        }

        private void OnChunkChange(ChunkChange chunkChange)
        {
            _chunkChanges.Enqueue(chunkChange);
        }

        private void Awake()
        {
            RebuildRendererDictionary();
        }


        private void RebuildRendererDictionary()
        {
            _chunkRenderers.Clear();

            ChunkRenderer[] renderers =
                GetComponentsInChildren<ChunkRenderer>();

            foreach (var chunkRenderer in renderers)
            {
                string rendererName = chunkRenderer.gameObject.name;
                
                ChunkCoord coord = ChunkCoord.ParseChunkCoord(rendererName);

                _chunkRenderers[coord] = chunkRenderer;
            }
        }

        private void Update() //todo use coroutine + timer
        {
            float3 viewerPosition = _viewer != null ? _viewer.position : float3.zero;
            _chunkManager?.Update(viewerPosition);
            SyncRenderers();
        }


        private void SyncRenderers()
        {
            while (_chunkChanges.Count > 0)
            {
                ChunkChange chunkChange = _chunkChanges.Dequeue();

                switch (chunkChange.Type)
                {
                    case ChunkChangeType.Load:
                        CreateRendererAt(chunkChange.Coord);
                        break;
                    case ChunkChangeType.Unload:
                        DestroyRendererAt(chunkChange.Coord);
                        break;
                    case ChunkChangeType.Update:
                        UpdateRendererAt(chunkChange.Coord);
                        break;
                }
            }
        }

        private void CreateRendererAt(ChunkCoord loadedChunkCoord)
        {
            
            MeshData meshData =
                _chunkManager.GetChunkAt(loadedChunkCoord)?.MeshData;
            
            if(meshData == null) return;
            
            GameObject go = new($"Chunk_{loadedChunkCoord}")
            {
                transform =
                {
                    parent = transform
                }
            };

            int3 worldPos = (loadedChunkCoord.Value * _chunkManager.ChunkSize);
            
            go.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z);

            ChunkRenderer chunkRenderer =
                go.AddComponent<ChunkRenderer>();
            

            Mesh unityMesh = UnityMeshBuilder.Build(meshData);

            chunkRenderer.ApplyMeshData(unityMesh);

            _chunkRenderers.Add(loadedChunkCoord, chunkRenderer);
        }

        private void DestroyRendererAt(ChunkCoord loadedChunkCoord)
        {
            ChunkRenderer chunkRenderer = _chunkRenderers[loadedChunkCoord];
            Destroy(chunkRenderer.gameObject);
            _chunkRenderers.Remove(loadedChunkCoord);
        }

        private void UpdateRendererAt(ChunkCoord loadedChunkCoord)
        {
            ChunkRenderer chunkRenderer = _chunkRenderers[loadedChunkCoord];
            MeshData meshData = _chunkManager.GetChunkAt(loadedChunkCoord)?.MeshData;
            if (meshData == null) return;
            Mesh unityMesh = UnityMeshBuilder.Build(meshData);
            chunkRenderer.ApplyMeshData(unityMesh);
        }
    }
}
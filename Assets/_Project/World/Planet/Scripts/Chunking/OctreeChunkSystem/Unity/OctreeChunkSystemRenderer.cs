using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Mathematics;
using UnityEngine;
// ReSharper disable MergeIntoNegatedPattern

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class OctreeChunkSystemRenderer : MonoBehaviour
    {
        private OctreeChunkManager _chunkManager;
        private Transform _viewer;

        [Header("Debug Settings")]
        [SerializeField, Range(0, 10)]
        private byte octreeGizmoDrawLayer;

        [SerializeField]
        private bool drawAllLayersUpToSelected;

        [SerializeField]
        private bool showStateCubes = true;

        [Header("State Cubes")]
        [SerializeField] private bool showFullStates;
        [SerializeField] private bool showEmptyStates;
        [SerializeField] private bool showMixedStates;
        [SerializeField] private bool showUnknownStates;

        private const int MaxInstancesPerBatch = 1023;

        private readonly Dictionary<byte, List<DebugOctreeBlock>> _debugOctreeBlocks = new();
        private readonly List<Matrix4x4> _instanceMatrices = new();
        
        private Mesh _cubeMesh;
        private Material _cubeMaterial;
        private bool _meshReady;

        public void SetChunkManager(OctreeChunkManager chunkManager)
        {
            _chunkManager = chunkManager;
        }

        private void Awake()
        {
            PrepareMesh();
        }

        private void Update()
        {
            float3 viewerPosition = _viewer != null
                ? _viewer.position
                : float3.zero;

            _chunkManager?.Update(viewerPosition);

            if (!_meshReady)
                return;

            bool needsRebuild =
                _debugOctreeBlocks.Count == 0 ||
                _lastShowEmptyStates != showEmptyStates ||
                _lastShowFullStates != showFullStates ||
                _lastShowMixedStates != showMixedStates ||
                _lastShowUnknownStates != showUnknownStates ||
                _lastDrawAllLayersUpToSelected != drawAllLayersUpToSelected ||
                _lastGizmoDrawLayer != octreeGizmoDrawLayer;

            if (needsRebuild)
            {
                RebuildOctreeBlocks();
            }

            if (showStateCubes)
            {
                RenderInstances();
            }
        }
        
        private bool _lastDrawAllLayersUpToSelected;
        private byte _lastGizmoDrawLayer;
        
        private bool _lastShowFullStates;
        private bool _lastShowEmptyStates;
        private bool _lastShowMixedStates;
        private bool _lastShowUnknownStates;


        private void RebuildOctreeBlocks()
        {
            if (_chunkManager == null || !_chunkManager.OctreeReady)
                return;

            _debugOctreeBlocks.Clear();
            _instanceMatrices.Clear();

            Octree octree = _chunkManager.Octree;
            byte maxDepth = octree.MaxDepth;

            foreach (OctreeNode node in octree.Nodes)
            {
                byte depth = node.MortonCode.GetDepth();
                
                if (drawAllLayersUpToSelected)
                {
                    if (depth > octreeGizmoDrawLayer) continue;
                }
                else
                {
                    if (depth != octreeGizmoDrawLayer) continue;
                }

                if (node.State == OctreeNodeState.Full && !showFullStates ||
                    node.State == OctreeNodeState.Empty && !showEmptyStates ||
                    node.State == OctreeNodeState.Mixed && !showMixedStates ||
                    node.State == OctreeNodeState.Unknown && !showUnknownStates)
                    continue;

                int nodeSize = 1 << (maxDepth - depth);

                int3 localGridPos = node.MortonCode.DecodeCoord();

                float3 worldMin = octree.Min + (localGridPos * nodeSize);

                float3 center = worldMin + (new float3(nodeSize) * 0.5f);

                var block = new DebugOctreeBlock
                {
                    Center = center,
                    Size = nodeSize,
                };

                if (!_debugOctreeBlocks.TryGetValue(depth, out var layer))
                {
                    layer = new List<DebugOctreeBlock>();
                    _debugOctreeBlocks.Add(depth, layer);
                }

                layer.Add(block);

                _instanceMatrices.Add(
                    Matrix4x4.TRS(
                        block.Center,
                        Quaternion.identity,
                        Vector3.one * block.Size
                    )
                );
            }
        }

        private void RenderInstances()
        {
            int totalCount = _instanceMatrices.Count;

            if (totalCount == 0)
                return;

            for (int i = 0; i < totalCount; i += MaxInstancesPerBatch)
            {
                int count = Mathf.Min(
                    MaxInstancesPerBatch,
                    totalCount - i
                );

                Matrix4x4[] batch = new Matrix4x4[count];

                _instanceMatrices.CopyTo(
                    i,
                    batch,
                    0,
                    count
                );

                Graphics.DrawMeshInstanced(
                    _cubeMesh,
                    0,
                    _cubeMaterial,
                    batch
                );
            }
        }

        private void PrepareMesh()
        {
            if (_meshReady)
                return;

            _cubeMesh = new Mesh
            {
                name = "Debug Cube",
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, -0.5f),
                    new Vector3( 0.5f, -0.5f, -0.5f),
                    new Vector3( 0.5f,  0.5f, -0.5f),
                    new Vector3(-0.5f,  0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f,  0.5f),
                    new Vector3( 0.5f, -0.5f,  0.5f),
                    new Vector3( 0.5f,  0.5f,  0.5f),
                    new Vector3(-0.5f,  0.5f,  0.5f)
                },
                triangles = new[]
                {
                    0,2,1, 0,3,2,
                    1,2,6, 1,6,5,
                    5,6,7, 5,7,4,
                    4,7,3, 4,3,0,
                    3,7,6, 3,6,2,
                    4,0,1, 4,1,5
                }
            };

            _cubeMesh.RecalculateNormals();
            _cubeMesh.RecalculateBounds();

            _cubeMaterial =
                new Material(
                    Shader.Find("Universal Render Pipeline/Lit")
                )
                {
                    enableInstancing = true
                };

            _meshReady = true;
        }

        private void OnDestroy()
        {
            if (_cubeMesh != null)
                Destroy(_cubeMesh);

            if (_cubeMaterial != null)
                Destroy(_cubeMaterial);
        }

        private struct DebugOctreeBlock
        {
            public float3 Center;
            public float Size;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen.Parallel;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
// ReSharper disable MergeIntoNegatedPattern

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    public static class OctreeHelper
    {
        public static OctreeNode? GetNodeAtPosition(this Octree octree, ulong mortonCode)
        {
            bool containsValue =
                octree.IndexLookup.TryGetValue(mortonCode,
                    out int nodeIndex); // get the position of the node with the given morton code in the node list by using the index lookup
            bool outOfBounds = nodeIndex >= octree.Nodes.Length;
            if (!containsValue || outOfBounds) return null;

            return octree.Nodes
                [nodeIndex]; // get the node at the given morton code by using the index lookup to find its position in the node list
        }

        public static Bounds GetBounds(this Octree octree, ulong mortonCode)
        {
            byte depth = mortonCode.GetDepth();

            int nodeSize = 1 << (octree.MaxDepth - depth);

            int3 localPos = mortonCode.DecodeToCoord();

            float3 min = octree.Min + localPos * nodeSize;
            float3 size = new float3(nodeSize);
            return new Bounds(
                min + size * 1.5f,
                size
            );
        }

        /// <summary>
        /// disposes the native collections of the octree. make sure to call this when you are done with the octree to avoid memory leaks. 
        /// </summary>
        /// <param name="octree">the octree to dispose</param>
        public static void Dispose(this Octree octree)
        {
            if (octree.Nodes.IsCreated) octree.Nodes.Dispose();
            if (octree.IndexLookup.IsCreated) octree.IndexLookup.Dispose();
        }
    }

    /// <summary>
    /// Manages asynchronous octree building with job scheduling.
    /// </summary>
    public class OctreeBuilder
    {
        private Octree Tree {get; }
        private readonly ParallelBurstSamplerSettings _settings;

        private readonly Queue<NativeList<ulong>> _pendingNodes;


        public OctreeBuilder(int3 min, byte maxDepth, ParallelBurstSamplerSettings settings)
        {
            _settings = settings;
            _pendingNodes = new Queue<NativeList<ulong>>();
            
            Tree = new Octree
            {
                Min = min,
                Max = min + new int3(1 << maxDepth),
                MaxDepth = maxDepth,
                Nodes = new NativeList<OctreeNode>(Allocator.Persistent),
                IndexLookup = new NativeHashMap<ulong, int>(1024, Allocator.Persistent)
            };
            
        }

        public void Build()
        {
            var octree = Tree;
            BuildNode(new int3(0, 0, 0).EncodeToMorton(0), new MinMaxValue(){Min = float.NegativeInfinity, Max = float.PositiveInfinity}, Tree.MaxDepth, ref octree);
        }

        private void BuildNode(ulong mortonCode, MinMaxValue minMaxValue, byte maxDepth, ref Octree tree)
        {
            byte depth = mortonCode.GetDepth();


            OctreeNodeState state = minMaxValue.Max < BurstMeshGenerator.IsoLevel
                ? OctreeNodeState.Full
                : minMaxValue.Min > BurstMeshGenerator.IsoLevel
                    ? OctreeNodeState.Empty
                    : OctreeNodeState.Mixed;
            
            bool isValidNode = depth <= maxDepth && state == OctreeNodeState.Mixed;

            OctreeNode node = new OctreeNode
            {
                MortonCode = mortonCode,
                State = state,
                ChildMask = isValidNode ? (byte)0xFF : (byte)0
            };

            AddNode(ref tree, node);

            if (depth >= maxDepth || state != OctreeNodeState.Mixed) return;

            for (int i = 0; i < 8; i++)
            {
                Enqueue(mortonCode.AppendChild((byte)i));
            }
        }

        private const int MaxJobBatchSize = 256;

        private void Enqueue(ulong mortonCode)
        {
            if (_pendingNodes.Count == 0 || _pendingNodes.Last().Length >= MaxJobBatchSize)
            {
                _pendingNodes.Enqueue(new NativeList<ulong>(Allocator.Persistent));
            }

            NativeList<ulong> newest = _pendingNodes.Last();
            newest.Add(mortonCode);
        }


        private JobHandle? _currentHandle;
        private NativeArray<MinMaxValue>? _currentMinMaxValues;
        private NativeList<ulong>? _currentMortons;

        public void Update()
        {
            var octree = Tree;
            CompleteCompletedNodePresets(ref octree);
            StartNewJobs();
        }

        public bool IsDone()
        {
            return _currentHandle == null && _pendingNodes.Count == 0;
        }

        public Octree? GetReadyTree()
        {
            if(!IsDone()) return null;
            return Tree;
        }

        private void StartNewJobs()
        {
            if (_currentHandle != null) return;

            while (_pendingNodes.Count > 0)
            {
                NativeList<ulong> currentNode = _pendingNodes.Dequeue();
                
                _currentMinMaxValues = new NativeArray<MinMaxValue>(currentNode.Length, Allocator.Persistent);

                var minMaxValues = _currentMinMaxValues ?? default;
                BurstSphericalNoiseClassificationJob job = _settings.CreateMinMaxSamplers(
                    currentNode,
                    Tree.Min,
                    5,
                    Tree.MaxDepth,
                    ref minMaxValues
                );
                _currentMinMaxValues = minMaxValues;

                _currentMortons = currentNode;

                _currentHandle = job.Schedule(currentNode.Length, 64);
            }
        }

        private void CompleteCompletedNodePresets(ref Octree tree)
        {
            if (_currentHandle == null)
                return;

            if (!_currentHandle.Value.IsCompleted)
                return;

            _currentHandle.Value.Complete();

            if (_currentMinMaxValues == null || _currentMortons == null)
                return;

            for (int i = _currentMinMaxValues.Value.Length - 1; i >= 0; --i)
            {
                ulong morton = _currentMortons.Value[i];
                BuildNode(morton, _currentMinMaxValues.Value[i], Tree.MaxDepth, ref tree);
            }

            _currentMinMaxValues.Value.Dispose();
            _currentMortons.Value.Dispose();

            _currentMinMaxValues = null;
            _currentMortons = null;
            _currentHandle = null;
        }

        private static void AddNode(ref Octree tree, OctreeNode node)
        {
            int index = tree.Nodes.Length;
            tree.Nodes.Add(node);
            tree.IndexLookup[node.MortonCode] = index;
        }

        public void Dispose()
        {
            _currentHandle?.Complete();
            _currentMinMaxValues?.Dispose();
            _currentMortons?.Dispose();

            while (_pendingNodes.Count > 0)
            {
                NativeList<ulong> nodeList = _pendingNodes.Dequeue();
                nodeList.Dispose();
            }
        }
    }
}
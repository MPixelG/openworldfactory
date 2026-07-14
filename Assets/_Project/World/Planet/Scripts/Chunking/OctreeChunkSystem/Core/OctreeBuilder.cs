using System.Collections.Generic;
using System.Linq;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen.Parallel;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    /// <summary>
    /// Manages asynchronous octree building with job scheduling.
    /// </summary>
    public class OctreeBuilder
    {
        private Octree _tree;
        private readonly ParallelBurstSamplerSettings _settings;

        private readonly Queue<NativeList<ulong>> _pendingNodes;


        public OctreeBuilder(int3 min, byte maxDepth, ParallelBurstSamplerSettings settings)
        {
            _settings = settings;
            _pendingNodes = new Queue<NativeList<ulong>>();
            
            _tree = new Octree
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
            BuildNode(new int3(0, 0, 0).EncodeToMorton(0), new MinMaxValue(){Min = float.NegativeInfinity, Max = float.PositiveInfinity}, _tree.MaxDepth);
        }
        
        private void BuildNode(ulong mortonCode, MinMaxValue minMaxValue, byte maxDepth)
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

            AddNode(node);
            if (!isValidNode) return;

            for (int i = 0; i < 8; i++)
            {
                Enqueue(mortonCode.AppendChild((byte)i));
            }
        }

        private const int MaxJobBatchSize = 262144; 
        private void Enqueue(ulong morton)
        {
            NativeList<ulong>? currentBatch = _pendingNodes.Count == 0 ? null : _pendingNodes.Last(); 
            if (currentBatch == null || currentBatch.Value.Length >= MaxJobBatchSize)
            {
                NativeList<ulong> newBatch = new NativeList<ulong>(Allocator.Persistent);
                newBatch.Add(morton);
                _pendingNodes.Enqueue(newBatch);
            } else currentBatch.Value.Add(morton); 
        }
        

        private readonly List<RunningJob> _runningJobs = new();
        
        public void Update()
        {
            CompleteCompletedNodePresets();
            StartNewJobs();
            Debug.Log("Current running jobs: " + _runningJobs.Count + ", pending nodes: " + _pendingNodes.Count);
        }

        public bool IsDone()
        {
            return _pendingNodes.Count == 0 &&
                   _runningJobs.Count == 0;
        }

        public Octree? GetReadyTree()
        {
            if(!IsDone()) return null;
            return _tree;
        }

        private const int SampleResolution = 5;
        private const int MaxConcurrentJobs = JobsUtility.MaxJobThreadCount;

        private void StartNewJobs()
        {
            while (_runningJobs.Count < MaxConcurrentJobs &&
                   _pendingNodes.Count > 0)
            {
                var mortons = _pendingNodes.Dequeue();

                var values = new NativeArray<MinMaxValue>(
                    mortons.Length,
                    Allocator.Persistent);

                var tmp = values;

                var job = _settings.CreateMinMaxSamplers(
                    mortons,
                    _tree.Min,
                    SampleResolution,
                    _tree.MaxDepth,
                    ref tmp);

                values = tmp;

                _runningJobs.Add(new RunningJob
                {
                    Handle = job.Schedule(mortons.Length, 64),
                    MinMaxValues = values,
                    Mortons = mortons
                });
            }
        }

        private void CompleteCompletedNodePresets()
        {
            for (int i = _runningJobs.Count - 1; i >= 0; i--)
            {
                var job = _runningJobs[i];

                if (!job.Handle.IsCompleted)
                    continue;

                job.Handle.Complete();

                for (int j = job.MinMaxValues.Length - 1; j >= 0; --j)
                {
                    BuildNode(job.Mortons[j],
                        job.MinMaxValues[j],
                        _tree.MaxDepth);
                }

                job.MinMaxValues.Dispose();
                job.Mortons.Dispose();

                _runningJobs.RemoveAt(i);
            }
        }

        private void AddNode(OctreeNode node)
        { 
            int index = _tree.Nodes.Length;
            _tree.Nodes.Add(node);
            _tree.IndexLookup[node.MortonCode] = index;
        }

        public void Dispose()
        {
            foreach (var job in _runningJobs)
            {
                job.Dispose();
            }

            while (_pendingNodes.Count > 0)
            {
                NativeList<ulong> nodeList = _pendingNodes.Dequeue();
                nodeList.Dispose();
            }
        }
        
        private struct RunningJob
        {
            public JobHandle Handle;
            public NativeArray<MinMaxValue> MinMaxValues;
            public NativeList<ulong> Mortons;

            public void Dispose()
            {
                Handle.Complete();
                if (MinMaxValues.IsCreated) MinMaxValues.Dispose();
                if (Mortons.IsCreated) Mortons.Dispose();
            }
        }
    }
}
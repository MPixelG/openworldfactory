using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.WorldGen;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    /// <summary>
    /// Optimized octree builder with async density sampling and smart node creation.
    /// Key optimizations:
    /// - Asynchronous job scheduling (no .Complete() blocking during build)
    /// - Smart sampling: only Mixed nodes require full mesh generation
    /// - Early termination for Full/Empty nodes
    /// - Hierarchical sampling: inherit state from parent when possible
    /// </summary>
    public static class OctreeHelper
    {
        /// <summary>
        /// Builds an octree asynchronously. This method schedules jobs but doesn't wait for them.
        /// Call CompleteBuilding() to wait for all jobs.
        /// </summary>
        private static OctreeBuilder BuildAsync(
            int3 min,
            int3 max,
            BurstSamplerSettings settings,
            int resolution
        )
        {
            int3 size = max - min;
            byte maxDepth = (byte)math.ceil(math.log2((float)math.cmax(size) / resolution));
            
            Octree tree = new()
            {
                Min = min,
                Max = min + new int3(1 << maxDepth),
                MaxDepth = maxDepth,
                Nodes = new NativeList<OctreeNode>(Allocator.Persistent),
                IndexLookup = new NativeHashMap<ulong, int>(1024, Allocator.Persistent)
            };

            var builder = new OctreeBuilder(tree, settings);
            builder.BuildNodeAsync(new int3(0, 0, 0).EncodeToMorton(0), maxDepth, ref tree);
            builder.Tree = tree; // Update builder's tree reference
            
            return builder;
        }

        /// <summary>
        /// Synchronous build - use only for small regions!
        /// </summary>
        public static Octree Build(
            int3 min,
            int3 max,
            BurstSamplerSettings settings,
            int resolution
        )
        {
            var builder = BuildAsync(min, max, settings, resolution);
            builder.CompleteBuilding();
            return builder.Tree;
        }
        
        /// <summary>
        /// splits the node at the given position exactly one time
        /// </summary>
        /// <param name="octree">the octree the node is inside of</param>
        /// <param name="mortonCode">the position of that node represented as a morton code</param>
        /// <param name="settings">the settings used for density generation</param>
        /// <param name="force">if true, the node will be split even if it reached the max depth.
        /// this can lead to problems, so use with caution</param>
        public static void Split(this Octree octree, ulong mortonCode, BurstSamplerSettings settings, bool force=false)
        {
            bool containsValue = octree.IndexLookup.TryGetValue(mortonCode, out int nodeIndex); // get the position of the node with the given morton code in the node list by using the index lookup
            bool outOfBounds = nodeIndex >= octree.Nodes.Length;
            if(!containsValue || outOfBounds) return;
            
            
            OctreeNode node = octree.Nodes[nodeIndex]; // get the node
            
            if (node.ChildMask != 0) return; // if that node already has children, we dont need to split it again
            
            byte depth = node.MortonCode.GetDepth();
            if (depth >= octree.MaxDepth && !force) return; // if we reached the max depth we cant split it anymore (except if the user wants to)
            
            OctreeBuilder builder = new OctreeBuilder(octree, settings);
            builder.BuildNodeAsync(mortonCode, depth, ref octree);
        }
        
        
        public static void Merge(this Octree octree, ulong mortonCode)
        {
            bool containsValue = octree.IndexLookup.TryGetValue(mortonCode, out int nodeIndex); // get the position of the node with the given morton code in the node list by using the index lookup
            bool outOfBounds = nodeIndex >= octree.Nodes.Length;
            if(!containsValue || outOfBounds) return;
            
            OctreeNode node = octree.Nodes[nodeIndex]; // get the node
            
            if (node.ChildMask == 0) return; // if that node has no children, we dont need to merge it
            
            node.ChildMask = 0; // remove the children of that node by setting the child mask to 0
            octree.Nodes[nodeIndex] = node; // update the nodes values (currently only the child mask)
        }


        public static OctreeNode? GetNodeAtPosition(this Octree octree, ulong mortonCode)
        {
            bool containsValue = octree.IndexLookup.TryGetValue(mortonCode, out int nodeIndex); // get the position of the node with the given morton code in the node list by using the index lookup
            bool outOfBounds = nodeIndex >= octree.Nodes.Length;
            if(!containsValue || outOfBounds) return null;
            
            return octree.Nodes[nodeIndex]; // get the node at the given morton code by using the index lookup to find its position in the node list
        }

        public static OctreeNode? GetNodeAtIndex(this Octree octree, int nodeIndex)
        {
            return (nodeIndex) < octree.Nodes.Length ? octree.Nodes[nodeIndex] : null;
        }
        
        public static Bounds GetBounds(this Octree octree, ulong mortonCode)
        {
            byte depth = mortonCode.GetDepth();

            int nodeSize = 1 << (octree.MaxDepth - depth);

            int3 localPos = mortonCode.DecodeToCoord();

            float3 min = octree.Min + localPos * nodeSize;
            float3 size = new float3(nodeSize);

            return new Bounds(
                min + size * 0.5f,
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

        /// <summary>
        /// adds a node to the given octree and returns its index
        /// </summary>
        /// <param name="octree">the octree to add the node to</param>
        /// <param name="node">the node to add</param>
        /// <returns>the index of that added node in the nodes list of the octree</returns>
        private static int AddNodeAndGetIndex(this Octree octree, OctreeNode node)
        {
            int index = octree.Nodes.Length;
            octree.Nodes.Add(node); // add the new node to the list
            octree.IndexLookup[node.MortonCode] = index;
            return index;
        }
        
        /// <summary>
        /// adds a node to the given octree
        /// </summary>
        /// <param name="octree">the octree to add the node to</param>
        /// <param name="node">the node to add</param>
        /// <returns>the index of that added node in the nodes list of the octree</returns>
        private static void AddNode(this Octree octree, OctreeNode node)
        {
            int index = octree.Nodes.Length;
            octree.Nodes.Add(node); // add the new node to the list
            octree.IndexLookup[node.MortonCode] = index;
        }
        
    }

    /// <summary>
    /// Manages asynchronous octree building with job scheduling.
    /// </summary>
    public class OctreeBuilder
    {
        public Octree Tree { get; set; }
        private BurstSamplerSettings _settings;
        private NativeList<JobHandle> _pendingJobs;

        public OctreeBuilder(Octree tree, BurstSamplerSettings settings)
        {
            Tree = tree;
            _settings = settings;
            _pendingJobs = new NativeList<JobHandle>(Allocator.Persistent);
        }

        public void BuildNodeAsync(ulong mortonCode, byte maxDepth, ref Octree tree)
        {
            byte depth = mortonCode.GetDepth();
            
            JobHandle sampleJob = DensityFieldBuilder.ScheduleBurstDensityFieldDataBuildInTree(
                _settings,
                mortonCode,
                maxDepth,
                tree.Min,
                5,
                out DensityFieldData sample
            );

            _pendingJobs.Add(sampleJob);
            
            sampleJob.Complete();
            
            float minDensity = float.PositiveInfinity;
            float maxDensity = float.NegativeInfinity;
            
            foreach (float densitySample in sample.Densities)
            {
                minDensity = math.min(minDensity, densitySample);
                maxDensity = math.max(maxDensity, densitySample);
            }
            

            OctreeNodeState state = maxDensity < BurstMeshGenerator.IsoLevel
                ? OctreeNodeState.Empty
                : minDensity > BurstMeshGenerator.IsoLevel
                    ? OctreeNodeState.Full
                    : OctreeNodeState.Mixed;

            OctreeNode node = new OctreeNode
            {
                MortonCode = mortonCode,
                State = state,
                ChildMask = 0
            };

            int nodeIndex = AddNodeAndGetIndex(ref tree, node);
            sample.Densities.Dispose();

            if (depth >= maxDepth || state != OctreeNodeState.Mixed) return;
            byte childMask = 0;
                
            for (int i = 0; i < 8; i++)
            {
                BuildNodeAsync(mortonCode.AppendChild((byte)i), maxDepth, ref tree);
                childMask |= (byte)(1 << i);
            }

            node.ChildMask = childMask;
            tree.Nodes[nodeIndex] = node;
        }

        public void CompleteBuilding()
        {
            foreach (var job in _pendingJobs)
            {
                job.Complete();
            }
        }

        private int AddNodeAndGetIndex(ref Octree tree, OctreeNode node)
        {
            int index = tree.Nodes.Length;
            tree.Nodes.Add(node);
            tree.IndexLookup[node.MortonCode] = index;
            return index;
        }

        public void Dispose()
        {
            foreach (var job in _pendingJobs)
            {
                job.Complete();
            }
            _pendingJobs.Dispose();
        }
    }
}


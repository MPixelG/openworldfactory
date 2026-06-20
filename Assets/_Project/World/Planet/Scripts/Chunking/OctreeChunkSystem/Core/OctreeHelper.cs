using System;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    public static class OctreeHelper
    {
        /// <summary>
        /// builds an octree in a given space with a given max depth using the given density generation settings. 
        /// </summary>
        /// <param name="min">the first corner of the space the octree will occupy</param>
        /// <param name="max">the other corner of the space the octree will occupy</param>
        /// <param name="settings">the settings used to generate the density values</param>
        /// <param name="resolution">the maximum depth the octree will go down to. be careful with this value though as it increases exponentially. 5 is normally already more than enough.</param>
        /// <returns>the generated octree</returns>
        public static Octree Build(
            int3 min,
            int3 max,
            BurstSamplerSettings settings,
            int resolution
        )
        {
            int3 size = max - min;
            byte maxDepth = (byte) math.ceil(math.log2(math.cmax(size / resolution))); // the max depth is calculated based on the size of the octree and the desired resolution. we want the smallest nodes to be at least as big as the resolution, so we calculate how many times we can divide the size by 2 until we reach the resolution. that gives us the max depth.
            
            Octree tree = new()
            {
                Min = min,
                Max = min + new int3(1 << maxDepth),
                MaxDepth = maxDepth,
                Nodes = new NativeList<OctreeNode>(Allocator.Persistent), // create a persistent dynamically sized native list containing the octree nodes
                IndexLookup = new NativeHashMap<ulong, int>(1024, Allocator.Persistent)
            };

            BuildNode( // we add the root node (which recursively builds its children) so that the tree has its content
                ref tree,
                MortonCodeHelper.Encode(new int3(0, 0, 0), 0),
                settings, // the current depth (of the root node) is always 0
                maxDepth
            );

            return tree;
        }

        /// <summary>
        /// recursively builds the children of the given node until the max depth is reached or the node is completely full or empty.
        /// it calculates the density values for the corners of the node and determines the state of the node based on those values.
        /// if the child node is mixed and the max depth is not reached, it will create that child node and recursively build it.
        /// </summary>
        /// <param name="tree">the tree that will contain the nodes</param>
        /// <param name="mortonCode"></param>
        /// <param name="settings">the settings for generating the density values</param>
        /// <param name="maxDepth">the maximum depth the tree will go to</param>
        /// <returns>the index of the build node</returns>
        private static void BuildNode(
            ref Octree tree,
            ulong mortonCode,
            BurstSamplerSettings settings,
            byte maxDepth
        )
        {
            byte depth = mortonCode.GetDepth();
            
            int nodeSize = 1 << (maxDepth - depth);
            
            int3 localGridPos = mortonCode.DecodeCoord();
            int3 min = tree.Min + (localGridPos * nodeSize); 
            int3 max = min + new int3(nodeSize);
            
            
            DensityFieldData sample = DensityFieldBuilder.BuildBurstDensityFieldDataInTree(
                settings,
                min,
                max,
                5 // we only need to sample the density at the corners and some samples in between to determine the state of the node. 5 is a good number for that.
            );

            float minDensity=float.PositiveInfinity;
            float maxDensity=float.NegativeInfinity;
            foreach (float densitySample in sample.Densities)
            {
                if(densitySample < minDensity) minDensity = densitySample;
                if(densitySample > maxDensity) maxDensity = densitySample;
            }
            
            OctreeNodeState state = maxDensity < BurstMeshGenerator.IsoLevel // if every value is below the isolevel the node is completely full
                ? OctreeNodeState.Full
                : minDensity > BurstMeshGenerator.IsoLevel // if every value is above the isolevel the node is completely empty
                    ? OctreeNodeState.Empty
                    : OctreeNodeState.Mixed; // else the values are above and below the isolevel so its mixed
            
            //todo if there are values below and above the isolevel in one quarter of a chunk we dont have to sample in that child chunk to see if its full empty since we know its mixed
            
            
            OctreeNode node = new OctreeNode
            {
                MortonCode = mortonCode,
                State = state,
                ChildMask = 0xFF // for now this is enough todo do better
            };
            
            tree.AddNode(node);
            
            if (depth >= maxDepth || state == OctreeNodeState.Full || state == OctreeNodeState.Empty) // if we reached the max depth or the node is completely full or empty,
                                                                                                      // we stop building and just add that node to the tree
            {
                node.ChildMask = 0;
                tree.Nodes[^1] = node; // update the node with the correct child mask (since it has no children)
                return;
            }


            for(int i = 0; i < 8; i++)
            {
                BuildNode( // recursively call that function for that child. it will stop as soon as its deep enough, since we provided a max depth and a current depth.
                    ref tree,
                    mortonCode.AppendChild((byte) i),
                    settings,
                    maxDepth
                );
            }
        }
        
        
        /// <summary>
        /// splits the node at the given position exactly one time
        /// </summary>
        /// <param name="octree">the octree the node is inside of</param>
        /// <param name="nodeIndex">the position of that node represented in the linear node list in the octree</param>
        /// <param name="settings">the settings used for density generation</param>
        /// <param name="force">if true, the node will be split even if it reached the max depth.
        /// this can lead to problems, so use with caution</param>
        public static void Split(this Octree octree, int nodeIndex, BurstSamplerSettings settings, bool force=false)
        {
            if (nodeIndex == -1) throw new Exception("Node not found"); // if there is no node with that morton code, we throw an exception
            
            OctreeNode node = octree.Nodes[nodeIndex]; // get the node
            
            if (node.ChildMask != 0) return; // if that node already has children, we dont need to split it again
            
            byte depth = node.MortonCode.GetDepth();
            if (depth >= octree.MaxDepth && !force) return; // if we reached the max depth we cant split it anymore (except if the user wants to)
            
            BuildNode(ref octree, node.MortonCode, settings, (byte)(depth + 1)); // build the children of that node (this will automatically stop at one layer
                                                                                 // below the current depth and when a node is full or empty)
            
            node.ChildMask = 0xFF; 
            octree.Nodes[nodeIndex] = node; // update the nodes values (currently only the child mask)
        }
        
        
        public static void Merge(this Octree octree, int nodeIndex)
        {
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
}
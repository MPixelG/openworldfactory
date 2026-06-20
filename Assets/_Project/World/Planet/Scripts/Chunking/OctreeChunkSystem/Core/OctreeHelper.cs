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
                FirstChildIndex = tree.Nodes.Length, 
                ChildMask = 0xFF // for now this is enough todo do better
            }; 
            
            if (depth >= maxDepth || state == OctreeNodeState.Full || state == OctreeNodeState.Empty) // if we reached the max depth or the node is completely full or empty,
                                                                                                      // we stop building and just add that node to the tree
            {
                node.ChildMask = 0;
                tree.AddNode(node);
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
            
            
            tree.AddNode(node);
        }
        
        
        /// <summary>
        /// splits the node at the given position exactly one time
        /// </summary>
        /// <param name="octree">the octree the node is inside of</param>
        /// <param name="nodeIndex">the position of that node represented in the linear node list in the octree</param>
        /// <param name="settings">the settings used for density generation</param>
        /// <param name="force">if true, the node will be split even if it reached the max depth. this can lead to problems if you try to split a node that is already at the max depth, so use with caution.</param>
        public static void Split(this Octree octree, int nodeIndex, BurstSamplerSettings settings, bool force=false)
        {
            if (nodeIndex == -1) throw new Exception("Node not found"); // if there is no node with that morton code, we throw an exception
            
            OctreeNode node = octree.Nodes[nodeIndex]; // get the node
            
            if (node.ChildMask != 0) return; // if that node already has children, we dont need to split it again
            
            byte depth = node.MortonCode.GetDepth();
            if (depth >= octree.MaxDepth && !force) return; // if we reached the max depth we cant split it anymore
            
            BuildNode(ref octree, node.MortonCode, settings, (byte)(depth + 1)); // build the children of that node (this will automatically stop at one layer below the current depth and when a node is full or empty)
            
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
        
        /*public static float DensityAt(this Octree tree, DensityStorage densityStorage, float3 position)
        {
            
            float3 localPos = (position - tree.Min) / (tree.Max - tree.Min); // a position from 0|0|0 to 1|1|1 within the octree or below / above if the position is outside the octree 

            if (localPos.x < 0 || localPos.y < 0 || localPos.z < 0 || localPos.x > 1 || localPos.y > 1 ||
                localPos.z > 1) return 0; // if the position is outside the bounds of the octree, return 0
            

            v2.OctreeNode currentNode = tree.Nodes[0]; // set the current node to the root node
            
            
            float3 currentLocalPos = localPos; // the current local pos will get scaled every layer we go down to always be between 0 and 1 within that node 
            while (currentNode.ChildMask != 0)
            {
                // the child index offset is the index of the child we want to go to.
                // we calculate it based on the current local position.
                // if the x value is above 0.5, we want to go to the right half of the node, so we add 1 to the child index offset.
                // if the y value is above 0.5, we want to go to the top half of the node, so we add 2 to the child index offset.
                // and if the z value is above 0.5, we want to go to the back half of the node, so we add 4 to the child index offset.
                // this way we can easily calculate the child index offset based on the current local position.
                int childIndexOffset = 
                    (currentLocalPos.x >= 0.5f ? 1 : 0) |
                    (currentLocalPos.y >= 0.5f ? 2 : 0) |
                    (currentLocalPos.z >= 0.5f ? 4 : 0);

                int childIndex = currentNode.FirstChildIndex + childIndexOffset; // the final child index (the index in the complete node list) is calculated by just adding the offset to the first child index
                
                currentNode = tree.Nodes[childIndex]; // set the current node to the child node (we traverse into that node)
                
                currentLocalPos = math.frac(currentLocalPos * 2f); // now we just need to convert the local space of the parent into the local space of the child.
                                                                   // we do this by multiplying the current local pos by 2. 
                                                                   // if the resulting position is above 1 we subtract 1. 
                                                                   // thats basically what the math.frac() function does.
            }

            v2.OctreeNodeState currentNodeState = currentNode.State;
            switch (currentNodeState)
            {
                case v2.OctreeNodeState.Empty: // if the complete node only consists of density values below the isolevel we can just return 0 since it will be below the isolevel anyway
                    return 0;
                case v2.OctreeNodeState.Full: // the same goes for a full node (only containing values above the isolevel)
                    return 1;
                case v2.OctreeNodeState.Mixed:
                {
                    DensityFieldData densityField = densityStorage.GetDensityFieldDataOf(currentNode.MortonCode);
                    float3 currentLocalPosUpscaled = currentLocalPos * densityField.Size; // currently the local pos is between 0 and 1. the positions in the density field range from 0 to the size of the density field,
                                                                                                      // so we need to upscale the local pos to get the correct position in the density field.

                    return densityField.DensityAt(currentLocalPosUpscaled); // this DensityAt function (with a float3 as an input) does the interpolation for us
                }
                
                case v2.OctreeNodeState.Unknown: // this shouldnt really happen (same goes for every other case) so we throw an exception.
                default: throw new ArgumentOutOfRangeException();
            }
        }*/
    }
}
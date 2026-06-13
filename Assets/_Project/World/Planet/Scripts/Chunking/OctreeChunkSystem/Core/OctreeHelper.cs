using System;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core.Densities;
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
        /// <param name="maxDepth">the maximum depth the octree will go down to. be careful with this value though as it increases exponentially. 5 is normally already more than enough.</param>
        /// <returns>the generated octree</returns>
        public static Octree Build(
            int3 min,
            int3 max,
            BurstSamplerSettings settings,
            byte maxDepth
        )
        {
            Octree tree = new()
            {
                Min = min,
                Max = max,
                Nodes = new NativeList<OctreeNode>(Allocator.Persistent) // create a persistent dynamically sized native list containing the octree nodes
            };

            BuildNode( // we add the root node (which recursively builds its children) so that the tree has its content
                ref tree,
                min,
                max,
                settings,
                0, // the current depth (of the root node) is always 0
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
        /// <param name="min">one corner of the octree node in world space</param>
        /// <param name="max">the other corner in world space</param>
        /// <param name="settings">the settings for generating the density values</param>
        /// <param name="depth">the current depth of that node</param>
        /// <param name="maxDepth">the maximum depth the tree will go to</param>
        /// <returns>the index of the build node</returns>
        private static int BuildNode(
            ref Octree tree,
            int3 min,
            int3 max,
            BurstSamplerSettings settings,
            byte depth,
            byte maxDepth
        )
        {
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
                Coord = min,
                Depth = depth,
                State = state,
                FirstChildIndex = tree.Nodes.Length, 
                ChildMask = 0
            };
            
            int nodeIndex = tree.AddNode(node);
            
            if (depth >= maxDepth || state == OctreeNodeState.Full || state == OctreeNodeState.Empty) // if we reached the max depth or the node is completely full or empty,
                                                                                                      // we stop building and just add that node to the tree
            {
                return nodeIndex;
            }

            int3 size = max - min;
            int3 half = size / 2;

            for(int i = 0; i < 8; i++)
            {
                int3 offset = GetChildOffset(i); // get the offset of that child node (in a range of 0 to 1).
                                                 // so for example corner 0 in the front left bottom would have an offset of 0|0|0,
                                                 // while the corner in the back right top would have an offset of 1|1|1. 

                int3 childMin =
                    min + offset * half; // now scaled that offset so that it represents the local spacing (so a higher depth = less space) and add the min to it to move it to the correct global pos

                int3 childMax = childMin + half;

                BuildNode( // recursively call that function for that child. it will stop as soon as its deep enough, since we provided a max depth and a current depth.
                    ref tree,
                    childMin,
                    childMax,
                    settings,
                    (byte)(depth + 1),
                    maxDepth
                );
            }
            
            node.ChildMask = 0xFF; // for now this is enough todo do better
            
            tree.Nodes[nodeIndex] = node; // update the nodes values (currently only the child mask)
            
            return nodeIndex; // and return the nodes index
        }

        /// <summary>
        /// the offset of that child node (in a range of 0 to 1).
        /// so for example corner 0 in the front left bottom would have an offset of 0|0|0, while the corner in the back right top would have an offset of 1|1|1. 
        /// </summary>
        /// <param name="index">the index of the corner</param>
        /// <returns>the offset (between 0|0|0 and 1|1|1, only full decimals)</returns>
        private static int3 GetChildOffset(int index)
        {
            return new int3
            {
                x = (index & 1) != 0 ? 1 : 0,
                y = (index & 2) != 0 ? 1 : 0,
                z = (index & 4) != 0 ? 1 : 0
            };
        }

        /// <summary>
        /// adds a node to the given octree
        /// </summary>
        /// <param name="octree">the octree to add the node to</param>
        /// <param name="node">the node to add</param>
        /// <returns>the index of that added node in the nodes list of the octree</returns>
        private static int AddNode(this Octree octree, OctreeNode node)
        {
            int index = octree.Nodes.Length; // the index of the new node is the current length of the node list (since we add the new node at the end of the list)
            octree.Nodes.Add(node); // add the new node to the list
            return index; // return the index of the new node
        }
        
        public static float DensityAt(this Octree tree, DensityStorage densityStorage, float3 position)
        {
            
            float3 localPos = (position - tree.Min) / (tree.Max - tree.Min); // a position from 0|0|0 to 1|1|1 within the octree or below / above if the position is outside the octree 

            if (localPos.x < 0 || localPos.y < 0 || localPos.z < 0 || localPos.x > 1 || localPos.y > 1 ||
                localPos.z > 1) return 0; // if the position is outside the bounds of the octree, return 0
            

            OctreeNode currentNode = tree.Nodes[0]; // set the current node to the root node
            
            
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

            OctreeNodeState currentNodeState = currentNode.State;
            switch (currentNodeState)
            {
                case OctreeNodeState.Empty: // if the complete node only consists of density values below the isolevel we can just return 0 since it will be below the isolevel anyway
                    return 0;
                case OctreeNodeState.Full: // the same goes for a full node (only containing values above the isolevel)
                    return 1;
                case OctreeNodeState.Mixed:
                {
                    NodeKey currentNodeKey = new NodeKey
                    {
                        Coord = currentNode.Coord,
                        Depth = currentNode.Depth
                    };
                    
                    DensityFieldData densityField = densityStorage.GetDensityFieldDataOf(currentNodeKey);
                    float3 currentLocalPosUpscaled = currentLocalPos * densityField.Size; // currently the local pos is between 0 and 1. the positions in the density field range from 0 to the size of the density field,
                                                                                                      // so we need to upscale the local pos to get the correct position in the density field.

                    return densityField.DensityAt(currentLocalPosUpscaled); // this DensityAt function (with a float3 as an input) does the interpolation for us
                }
                
                case OctreeNodeState.Unknown: // this shouldnt really happen (same goes for every other case) so we throw an exception.
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
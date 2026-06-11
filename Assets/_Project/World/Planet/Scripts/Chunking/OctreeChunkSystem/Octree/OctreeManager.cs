using System;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Octree
{
    public static class OctreeManager
    {

        public static float DensityAt(this Octree tree, float3 position)
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
                    float3 currentLocalPosUpscaled = currentLocalPos * currentNode.DensityField.Size; // currently the local pos is between 0 and 1. the positions in the density field range from 0 to the size of the density field,
                                                                                                      // so we need to upscale the local pos to get the correct position in the density field.

                    return currentNode.DensityField.DensityAt(currentLocalPosUpscaled); // this DensityAt function (with a float3 as an input) does the interpolation for us
                }
                
                case OctreeNodeState.Unknown: // this shouldnt really happen (same goes for every other case) so we throw an exception.
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}
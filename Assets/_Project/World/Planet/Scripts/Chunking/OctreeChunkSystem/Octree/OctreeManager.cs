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
            
            
            float3 currentLocalPos = localPos;
            while (currentNode.ChildMask != 0)
            {
                int childIndexOffset =
                    (currentLocalPos.x >= 0.5f ? 1 : 0) |
                    (currentLocalPos.y >= 0.5f ? 2 : 0) |
                    (currentLocalPos.z >= 0.5f ? 4 : 0);

                int childIndex = currentNode.FirstChildIndex + childIndexOffset;
                
                currentNode = tree.Nodes[childIndex];
                
                currentLocalPos = math.frac(currentLocalPos * 2f);
            }

            OctreeNodeState currentNodeState = currentNode.State;
            switch (currentNodeState)
            {
                case OctreeNodeState.Empty:
                    return 0;
                case OctreeNodeState.Full:
                    return 1;
                case OctreeNodeState.Mixed:
                {
                    float3 currentLocalPosUpscaled = currentLocalPos * currentNode.Resolution;

                    return currentNode.DensityField.DensityAt(currentLocalPosUpscaled);
                }
                
                case OctreeNodeState.Unknown: return 0;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}
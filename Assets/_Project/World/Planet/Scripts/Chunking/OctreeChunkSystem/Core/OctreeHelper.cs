using Unity.Mathematics;
using UnityEngine;

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

            return octree.Nodes[nodeIndex]; // get the node at the given morton code by using the index lookup to find its position in the node list
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
    }
}
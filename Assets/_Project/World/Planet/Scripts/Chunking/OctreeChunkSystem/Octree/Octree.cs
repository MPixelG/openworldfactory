using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Octree
{
    /// <summary>
    /// a struct that represents an octree. it contains a list of octree nodes and the min and max bounds of the octree.
    /// the octree nodes are stored in a flat list and the children of each node are stored as indices in the list.
    /// this allows for efficient traversal of the octree without the need for pointers or references.
    /// </summary>
    public struct Octree
    {
        public NativeList<OctreeNode> Nodes;
        public float3 Min;
        public float3 Max; 
    }
}
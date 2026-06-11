using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Octree
{
    public struct Octree
    {
        public NativeList<OctreeNode> Nodes;
        public float3 Min;
        public float3 Max; 
    }
}
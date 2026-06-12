using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    public struct NodeKey
    {
        public int3 Coord;
        public int Depth;
    }
}
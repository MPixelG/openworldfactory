using System;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    public struct NodeKey : IEquatable<NodeKey>
    {
        public int3 Coord;
        public int Depth;

        public bool Equals(NodeKey other)
        {
            return Coord.Equals(other.Coord) && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coord, Depth);
        }
    }
}
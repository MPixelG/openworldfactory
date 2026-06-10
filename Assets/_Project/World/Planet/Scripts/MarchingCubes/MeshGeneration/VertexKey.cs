using System;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public readonly struct VertexKey : IEquatable<VertexKey>
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;
        private readonly byte _edge;

        public VertexKey(int3 cell, byte edge)
        {
            _x = cell.x;
            _y = cell.y;
            _z = cell.z;
            _edge = edge;
        }

        public bool Equals(VertexKey other)
            => _x == other._x && _y == other._y && _z == other._z && _edge == other._edge;

        public override int GetHashCode()
        {
            return (int)math.hash(new int4(_x, _y, _z, _edge));
        }
    }
}
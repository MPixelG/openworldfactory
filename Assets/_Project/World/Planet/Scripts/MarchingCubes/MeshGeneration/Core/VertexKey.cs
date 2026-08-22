using System;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public struct VertexKey : IEquatable<VertexKey>
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;
        private readonly int _x2;
        private readonly int _y2;
        private readonly int _z2;

        public VertexKey(int3 e1, int3 e2)
        {
            _x = e1.x;
            _y = e1.y;
            _z = e1.z;
            
            _x2 = e2.x;
            _y2 = e2.y;
            _z2 = e2.z;
        }

        public bool Equals(VertexKey other)
            => _x == other._x && _y == other._y && _z == other._z && _x2 == other._x2 && _y2 == other._y2 &&
               _z2 == other._z2;

        public override int GetHashCode()
        {
            return (int)math.hash(new uint2(math.hash(new int3(_x, _y, _z)), math.hash(new int3(_x, _y, _z))));
        }
    }
}
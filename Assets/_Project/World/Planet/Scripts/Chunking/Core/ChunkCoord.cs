using System;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.Core
{
    public readonly struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public readonly int3 Value;

        public ChunkCoord(int3 value)
        {
            Value = value;
        }
        public ChunkCoord(int x, int y, int z)
        {
            Value = new int3(x, y, z);
        }

        public static implicit operator int3(ChunkCoord c) => c.Value;
        public static implicit operator ChunkCoord(int3 v) => new(v);
    
        public int X => Value.x;
        public int Y => Value.y;
        public int Z => Value.z;

        public bool Equals(ChunkCoord other) => Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is ChunkCoord other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
        public static bool operator !=(ChunkCoord a, ChunkCoord b) => !a.Equals(b);


        public static ChunkCoord ParseChunkCoord(string coord)
        {
            string[] coords = coord.Replace("ChunkCoord(", "").Replace(")", "").Split(',');
            return new ChunkCoord(int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]));
        }

        public override string ToString() => $"ChunkCoord({X}, {Y}, {Z})";
    }
}
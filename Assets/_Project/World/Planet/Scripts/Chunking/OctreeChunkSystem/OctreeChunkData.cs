using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem
{
    public class OctreeChunkData
    {
        public int3 Coord;
        public byte Depth;
        //public MeshData MeshData;
        //[ReadOnly]
        //public DensityFieldData DensityField;
        public OctreeNodeState State;
    }
}
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Octree
{
    public struct OctreeNode
    {
        public int FirstChildIndex;
        public byte ChildMask;

        public short Resolution;

        public OctreeNodeState State;

        public DensityFieldData DensityField;
    }

    public enum OctreeNodeState
    {
        Unknown,
        Empty,
        Full,
        Mixed
    }
}
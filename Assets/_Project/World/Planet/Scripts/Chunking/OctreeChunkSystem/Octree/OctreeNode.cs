using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Octree
{
    public struct OctreeNode
    {
        public int FirstChildIndex; // since the nodes are stored linear in one big list on one layer we can store the index of the first child
        
        public byte ChildMask; // an octree node can have up to 8 children. every bit of this byte represents one child.
                               // so 0b01010001 would indicate that that node has a bottom left front, top left front and top left back
                               // child, but no other children. this is how i chose to count the corners:
                               // 
                               //    6--------7
                               //   /|       /|
                               //  / |      / |
                               // 4--------5  |
                               // |  |     |  |
                               // |  2-----|--3
                               // | /      | /
                               // |/       |/
                               // 0--------1

        public OctreeNodeState State; // the state of the node is either empty, full, mixed or unknown.
                                      // if its empty, it means that the density field is below the isolevel everywhere in that node.
                                      // if its full, it means that the density field is above the isolevel everywhere in that node.
                                      // if its mixed, it means that the density field is above the isolevel in some places and below it in others.
                                      // and if its unknown, it means that we haven't sampled the density field for that node yet. 
                                      // this way we can easily skip over completely empty or completely full density fields when we mesh it since theres only a mesh if the isosurface slices through that chunk

        public DensityFieldData DensityField; // the density field contains the density data of that node
    }

    public enum OctreeNodeState
    {
        Unknown,
        Empty,
        Full,
        Mixed
    }
}
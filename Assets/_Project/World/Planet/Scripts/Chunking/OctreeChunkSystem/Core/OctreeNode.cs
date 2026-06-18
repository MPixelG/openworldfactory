namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    public struct OctreeNode
    {
        /// <summary>
        /// the morton code of the node is a unique identifier for the node that encodes its position in the octree. it is calculated by using 3 bits per layer.
        /// since every node has 8 child nodes, we can use 3 bits to encode which child node it is. so the first 3 bits of the morton code would indicate which child node
        /// of the root node it is, the next 3 bits would indicate which child node of that node it is and so on.
        /// this way we can easily calculate the morton code of any node by just shifting the bits and adding the child index at each layer. 
        /// for example, you could represent a morton code of 0b010110011101 as 2535, indicating that its child index 2, then 5, then 3 and then 5 of the root node.
        /// and theres another problem that might occur. since hashing (and thus the use in a hash map as a key for example) is way faster if only one ulong is used we need to also store the depth in that morton code
        /// however theres a problem. for the computer 0b000000 and 0b000000000 is the same, even though the second one has a deeper depth.
        /// we can resolve that problem by adding a leading 1 (sentinel bit) at the end of our code. so it would be 0b0000001 and 0000000001, so the computer knows the difference. 
        /// </summary>
        public ulong MortonCode; 


        /// <summary>
        /// since the nodes are stored linearly in one big list on one layer we can store the index of the first child
        /// </summary>
        public int FirstChildIndex;

        /// <summary>
        /// an octree node can have up to 8 children. every bit of this byte represents one child.
        /// so 0b01010001 would indicate that that node has a bottom left front, top left front and top left back
        /// child, but no other children. this is how i chose to count the corners:
        /// <code>
        ///    6--------7
        ///   /|       /|
        ///  / |      / |
        /// 4--------5  |
        /// |  |     |  |
        /// |  2-----|--3
        /// | /      | /
        /// |/       |/
        /// 0--------1
        /// </code>
        /// </summary>
        public byte ChildMask; 

        /// <summary>
        /// the state of the node is either empty, full, mixed or unknown.
        /// if its empty, it means that the density field is below the isolevel everywhere in that node.
        /// if its full, it means that the density field is above the isolevel everywhere in that node.
        /// if its mixed, it means that the density field is above the isolevel in some places and below it in others.
        /// and if its unknown, it means that we haven't sampled the density field for that node yet. 
        /// this way we can easily skip over completely empty or completely full density fields when we mesh it since theres only a mesh if the isosurface slices through that chunk
        /// </summary>
        public OctreeNodeState State;
    }

    public enum OctreeNodeState : byte
    {
        Unknown,
        Empty,
        Full,
        Mixed
    }
}
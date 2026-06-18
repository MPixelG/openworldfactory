using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core
{
    public static class MortonCodeHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ExpandBits(uint v)
        {
            ulong x = v & 0x1FFFFFul; // only use the lowest 21 bits, since we can only encode up to 2^21 - 1 in each axis
            x = (x | (x << 32)) & 0x1F00000000FFFFul;
            x = (x | (x << 16)) & 0x1F0000FF0000FFul;
            x = (x | (x << 8)) & 0x100F00F00F00F00Ful;
            x = (x | (x << 4)) & 0x10C30C30C30C30C3ul;
            x = (x | (x << 2)) & 0x1249249249249249ul;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CompactBits(ulong x)
        {
            x &= 0x1249249249249249ul;
            x = (x ^ (x >> 2)) & 0x10C30C30C30C30C3ul;
            x = (x ^ (x >> 4)) & 0x100F00F00F00F00Ful;
            x = (x ^ (x >> 8)) & 0x1F0000FF0000FFul;
            x = (x ^ (x >> 16)) & 0x1F00000000FFFFul;
            x = (x ^ (x >> 32)) & 0x1FFFFFul;
            return (uint)x;
        }
        

        /// <summary>
        /// returns the morton code for the given 3d coordinate and depth. the coordinate should be in the range of [0, 2^depth) for each axis, where depth is the maximum depth of the octree.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Encode(int3 coord, byte depth)
        {
            // we can encode the 3d coordinate into a morton code by interleaving the bits of the x, y and z coordinates
            ulong morton = ExpandBits((uint)coord.x) |
                           (ExpandBits((uint)coord.y) << 1) |
                           (ExpandBits((uint)coord.z) << 2);

            // set the sentinel bit to the left of the last used bit to encode the depth
            ulong sentinel = 1ul << (depth * 3);

            return morton | sentinel;
        }

        /// <summary>
        /// returns a 3d coordinate from that location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 DecodeCoord(this ulong locationCode)
        {
            // remove the sentinel bit
            int highestBitIndex = 63 - math.lzcnt(locationCode);
            ulong mask = (1ul << highestBitIndex) - 1ul; // a mask that sets all bits below that sentinel bit to 1 and all bits above to 0
            ulong rawMorton = locationCode & mask;

            // now we can extract the x, y and z coordinates by compacting the bits. since we know that the x bits are at position 0, 3, 6... we can just shift the raw morton code to the right by 0, 1 and 2 bits and then compact the bits to get the original coordinates.
            return new int3(
                (int)CompactBits(rawMorton),
                (int)CompactBits(rawMorton >> 1),
                (int)CompactBits(rawMorton >> 2)
            );
        }

        /// <summary>
        /// reads the depth of that location extremely efficiently. make sure the location code contains a sentinel bit.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetDepth(this ulong locationCode)
        {
            int highestBitIndex = 63 - math.lzcnt(locationCode); //lzcnt returns the number of leading zeros, so we subtract that from 63 to get the index of the highest set bit (the sentinel bit).
            return (byte)(highestBitIndex / 3);
        }

        /// <summary>
        /// returns the morton code of the child node
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetParent(this ulong locationCode)
        {
            // by shifting the code 3 bits to the right, we effectively remove the last child index and move the sentinel bit down to the new depth level.
            return locationCode >> 3;
        }

        /// <summary>
        /// adds a new child index and returns the morton code of the child node. the child index is a value between 0 and 7 that indicates which child node it is.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AppendChild(this ulong locationCode, byte childIndex)
        {
            // make space for the 3 new bits and add the child index. the sentinel bit is already in the correct position since it moves up with every shift.
            return (locationCode << 3) | childIndex;
        }
    }
}
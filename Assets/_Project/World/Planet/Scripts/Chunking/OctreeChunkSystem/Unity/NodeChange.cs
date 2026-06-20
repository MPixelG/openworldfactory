using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    /// <summary>
    /// a chunk change is a change the chunk manager made. this is used to signal the unity side of the project that doesnt have direct access to the data what changes have been made so that the graphics get updated correctly. 
    /// </summary>
    public struct NodeChange
    {

        public ulong MortonCode;
        public NodeChangeType Type;

        public NodeChange(ulong mortonCode, NodeChangeType type)
        {
            MortonCode = mortonCode;
            Type = type;
        }

        /// <summary>
        /// the type of the ChunkChange. 
        /// </summary>
        public enum NodeChangeType
        {
            Load,
            Unload,
            Update
        }
    }
}
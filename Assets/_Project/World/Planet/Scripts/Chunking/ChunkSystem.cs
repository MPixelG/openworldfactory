using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking
{
    /// <summary>
    /// the base for all different chunk systems. so both a tessellated chunk system and a grid based chunk system have to contain the methods GetChunkAt and SetChunkAt. 
    /// </summary>
    public abstract class ChunkSystem : MonoBehaviour
    {
        public abstract Chunk GetChunkAt(Vector3Int position);
        public abstract void SetChunkAt(Vector3Int position, Chunk chunk);
    }
}
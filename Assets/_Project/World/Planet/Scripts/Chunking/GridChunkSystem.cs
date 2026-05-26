using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking
{
    [ExecuteAlways]
    public class GridChunkSystem : MonoBehaviour
    {
        private GridChunkManager _manager;
        private ChunkFactory _chunkFactory;
        
        [Header("Chunk Settings"), SerializeField, Range(1, 64)]
        private int chunkSize = 16;
        
        //[Header("World Gen"), SerializeReference]
        private IDensitySampler _densitySampler;
        
        [SerializeField, ]
        private Vector3Int worldSize = new(6, 6, 6);
        
        private void Awake()
        {
            _manager = new GridChunkManager();
            _densitySampler = ScriptableObject.CreateInstance<TerrainNoiseGenerator>();
            _chunkFactory = new ChunkFactory(_densitySampler, chunkSize);
            RegenerateChunks();
        }

        private void RegenerateChunks()
        {
            for (int x = 0; x < worldSize.x; x++)
            {
                for (int y = 0; y < worldSize.y; y++)
                {
                    for (int z = 0; z < worldSize.z; z++)
                    {
                        int3 chunkCoord = new int3(x, y, z);
                        ChunkData data = _chunkFactory.GenerateChunkAt(chunkCoord);
                        GameObject chunkObject = new($"Chunk_{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}")
                            {
                                transform =
                                {
                                    parent = transform,
                                    localPosition = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, chunkCoord.z * chunkSize)
                                }
                            };
                        Chunk chunk = chunkObject.AddComponent<Chunk>();
                        chunk.Data = data;
                        chunk.ApplyMeshData(UnityMeshBuilder.Build(data.MeshData));
                        _manager.SetChunkAt(chunkCoord, chunk);
                        Debug.Log("Chunk Regenerated at " + chunkCoord);
                    }
                }
            }
        }
    }
}
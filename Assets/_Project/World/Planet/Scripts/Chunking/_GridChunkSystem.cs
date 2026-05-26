/*using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Project.World.Planet.Scripts.MarchingCubes;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace _Project.World.Planet.Scripts.Chunking
{
    [ExecuteAlways]
    public class _GridChunkSystem : ChunkSystem
    {
        [Header("Chunk settings")]
        [SerializeField] private _MarchingCubesChunk chunkPrefab; // this is basically the pattern for a chunk. the chunk system will instantiate this prefab for every chunk it needs to create and then configure it with the right settings
        [SerializeField, Range(1, 256)] private int chunkSize = 16; // the grid size for every chunk
        [SerializeField] private Vector3Int gridStart = Vector3Int.zero; // the origin of the chunk system (in chunk coordinates!)
        [SerializeField] private Vector3Int gridDimensions = Vector3Int.one; // the size of the chunk system

        [Header("Terrain settings")]
        [SerializeReference] private TerrainGenerator terrainGenerator; // the generator used for generating the different chunks

        private readonly Dictionary<Vector3Int, Chunk> _chunks = new(); // the map that actually contains every chunk

        //the last used settings so that the system can know if something changed and it needs to rebuild
        private Vector3Int _lastGridStart; 
        private Vector3Int _lastGridDimensions;
        private int _lastChunkSize;
        private TerrainGenerator _lastGenerator;
        private _MarchingCubesChunk _lastChunkPrefab;
        private bool _isBuilding; // whether the system is currently rebuilding. 

        private void OnEnable() // initially build the chunks
        {
            if (!CanBuildInCurrentContext()) return; 
            CacheExistingChunks();
            if (!HaveSettingsChanged()) return;
            BuildGrid();
        }

        private void OnValidate() // every time the user changed a setting
        {
            if (Application.isPlaying) return;
            if (!CanBuildInCurrentContext()) return;
            if (_isBuilding) return;
            if (!HaveSettingsChanged()) return;
            BuildGrid();
        }

        private bool CanBuildInCurrentContext()
        {
            if (!gameObject.scene.IsValid() || !gameObject.scene.isLoaded)
                return false;
            #if UNITY_EDITOR
            return PrefabStageUtility.GetCurrentPrefabStage() == null;
            #endif
        }

        /// <summary>
        /// this revalidates the currently stored chunks. if the user deleted / added chunks manually in the editor, this will update the chunk map
        /// </summary>
        private void CacheExistingChunks() 
        {
            _chunks.Clear();
            foreach (var chunk in GetComponentsInChildren<_MarchingCubesChunk>(true))
            {
                _chunks[chunk.ChunkCoord] = chunk;
            }
        }

        private void CacheSettings()
        {
            _lastGridStart = gridStart;
            _lastGridDimensions = gridDimensions;
            _lastChunkSize = chunkSize;
            _lastGenerator = terrainGenerator;
            _lastChunkPrefab = chunkPrefab;
        }

        public override Chunk GetChunkAt(Vector3Int position)
        {
            _chunks.TryGetValue(position, out var chunk);
            return chunk;
        }

        public override void SetChunkAt(Vector3Int position, Chunk chunk)
        {
            _chunks[position] = chunk;
        }

        /// <summary>
        /// This is the main function of the chunk system. it checks which chunks should be present based on the current settings and creates / updates / deletes chunks as necessary
        /// </summary>
        private void BuildGrid()
        {
            if (!CanBuildInCurrentContext()) return;
            if (chunkPrefab == null || terrainGenerator == null) return; 

            _isBuilding = true; // mark the system is currently building so it doesnt start a new task in parallel
            try
            {
                var desiredCoords = new HashSet<Vector3Int>(); // this will store all the chunk coordinates that should be present based on the current settings. we will also use this to determine which chunks we need to remove because they are no longer in the desired area
                for (int x = 0; x < gridDimensions.x; x++)
                {
                    for (int y = 0; y < gridDimensions.y; y++)
                    {
                        for (int z = 0; z < gridDimensions.z; z++)
                        {
                            var coord = new Vector3Int(gridStart.x + x, gridStart.y + y, gridStart.z + z);
                            desiredCoords.Add(coord);
                            EnsureChunkAt(coord);
                        }
                    }
                }

                var toRemove = (from kvp in _chunks where !desiredCoords.Contains(kvp.Key) select kvp.Key).ToList(); // this stores the chunk coords that the system wont keep. its calculated by looking what chunks are not in the desired chunks set and if it doesnt contain that coord the chunk isnt needed

                foreach (var coord in toRemove) // remove every chunk we dont need to keep
                {
                    if (_chunks.TryGetValue(coord, out var chunk) && chunk != null) 
                    {
                        if (Application.isPlaying)
                            Destroy(chunk.gameObject);
                        else
                            DestroyImmediate(chunk.gameObject);
                    }
                    _chunks.Remove(coord);
                }
            }
            finally
            {
                CacheSettings();
                _isBuilding = false;
            }
        }

        private void EnsureChunkAt(Vector3Int coord)
        {
            Vector3 worldMin = gridStart * chunkSize;
            Vector3 worldMax = worldMin + gridDimensions * chunkSize;
            
            if (_chunks.TryGetValue(coord, out var existing))
            {
                if (existing is not _MarchingCubesChunk existingChunk) return;
                

                
                existingChunk.Configure(
                    coord,
                    chunkSize,
                    terrainGenerator,
                    worldMin,
                    worldMax
                );
                return;
            }

            _MarchingCubesChunk chunk = Instantiate(chunkPrefab, transform);
            chunk.name = $"Chunk_{coord.x}_{coord.y}_{coord.z}";
            chunk.Configure(
                coord,
                chunkSize,
                terrainGenerator,
                worldMin,
                worldMax
            );


            _chunks[coord] = chunk;
        }

        private bool HaveSettingsChanged()
        {
            return _lastGridStart != gridStart
                   || _lastGridDimensions != gridDimensions
                   || _lastChunkSize != chunkSize
                   || _lastGenerator != terrainGenerator
                   || _lastChunkPrefab != chunkPrefab;
        }
    }
}*/
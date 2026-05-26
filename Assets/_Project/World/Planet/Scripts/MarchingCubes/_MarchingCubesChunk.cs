/*using System;
using _Project.World.Planet.Scripts.Chunking;
using _Project.World.Planet.Scripts.MarchingCubes.Core;
using _Project.World.Planet.Scripts.WorldGen;
using UnityEngine;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes
{
    [ExecuteAlways]
    public class _MarchingCubesChunk : Chunk
    {
        private static readonly int Cull = Shader.PropertyToID("_Cull");

        [Header("Chunk settings"), Range(1, 64)] [SerializeField]
        private int size = 4;

        [SerializeField]
        private Vector3Int chunkCoord = Vector3Int.zero;

        [Header("Noise settings")]
        [SerializeReference] private TerrainGenerator terrainGenerator;

        private MarchingCubesGrid _grid;
        private Vector3 _worldMin;
        private Vector3 _worldMax;

        private TerrainGenerator _subscribedGenerator;

        // the last values so that it knows if something changed
        private int _lastSize;
        private Vector3Int _lastChunkCoord;

        private void Awake()
        {
            EnsureComponents(); // ensure the game object contains a mesh filter and renderer
        }

        private bool _suppressValidate;
        private void OnValidate()
        {
            if (_suppressValidate) return;
            if (Application.isPlaying) return;
            if (!HasSettingsChanged()) return;
            Rebuild();
        }

        // whether the user changed the settings and the applied mesh is outdated
        private bool HasSettingsChanged()
        {
            return Math.Abs(_lastSize - size) > 0
                   || _lastChunkCoord != chunkCoord;
        }

        //ensures the components (currently the mesh filter and renderer) are applied to the game object
        private void EnsureComponents()
        {
            if (MeshFilter == null)
                MeshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            if (MeshRenderer == null)
                MeshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();

            if (MeshRenderer.sharedMaterial != null) return;
            
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Standard");
            MeshRenderer.sharedMaterial = new Material(shader);
            MeshRenderer.sharedMaterial.SetInt(Cull, (int)UnityEngine.Rendering.CullMode.Back);
        }

        private bool _rebuilding;
        //rebuilds the mesh
        private void Rebuild()
        {
            if (terrainGenerator == null) return;
            Debug.Log("Rebuilding MarchingCubesChunk at " + transform.position);
            if (_rebuilding) return; // if we are already rebuilding we dont want to start another rebuild task
            _rebuilding = true;

            EnsureComponents(); //ensure there is a mesh filter and renderer we can apply the mesh to

            Vector3 pos = new Vector3(chunkCoord.x, chunkCoord.y, chunkCoord.z) * size;
            _grid = new MarchingCubesGrid(size, terrainGenerator, pos, _worldMin, _worldMax); //generate a grid of density values
            MeshFilter.sharedMesh = MarchingCubesGenerator.GenerateMesh(_grid); //and apply it to the mesh filter

            // save the last versions of the iso level and size so that we know if the values updated
            _lastSize = size;
            _lastChunkCoord = chunkCoord;

            _rebuilding = false;
        }

        public void Configure(Vector3Int coord, int chunkSize, TerrainGenerator generator, Vector3 worldMin, Vector3 worldMax)
        {
            _suppressValidate = true;
            chunkCoord = new Vector3Int(coord.x, coord.y, coord.z);
            size = chunkSize;
            _worldMin = worldMin;
            _worldMax = worldMax;
            SubscribeGenerator(generator);
            transform.position = new Vector3(chunkCoord.x, chunkCoord.y, chunkCoord.z) * size;
            _suppressValidate = false;
            Rebuild();
        }

        private void SubscribeGenerator(TerrainGenerator generator)
        {
            if (_subscribedGenerator != null)
                _subscribedGenerator.OnSettingsChanged -= Rebuild;

            _subscribedGenerator = generator;
            terrainGenerator = generator;

            if (_subscribedGenerator != null)
                _subscribedGenerator.OnSettingsChanged += Rebuild;
        }

        private void OnEnable()
        {
            SubscribeGenerator(terrainGenerator);
        }

        private void OnDisable()
        {
            if (_subscribedGenerator != null)
                _subscribedGenerator.OnSettingsChanged -= Rebuild;
        }

        public Vector3Int ChunkCoord => new(chunkCoord.x, chunkCoord.y, chunkCoord.z);
    }
}*/
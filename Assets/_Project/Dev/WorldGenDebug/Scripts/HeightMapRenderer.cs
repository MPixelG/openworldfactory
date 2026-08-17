using System;
using System.Collections.Generic;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Project.Dev.WorldGenDebug.Scripts
{
    /// <summary>
    /// Generates a spherical terrain preview mesh by sampling the same world-gen noise as the 3D pipeline.
    /// Triangles are split into submeshes by dominant voxel material.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class HeightMapRenderer : MonoBehaviour
    {
        [SerializeField] private WorldGenDebugVisualizer visualizer;
        [SerializeField, Min(8)] private int longitudeSegments = 128;
        [SerializeField, Min(4)] private int latitudeSegments = 64;
        [SerializeField] private float heightScale = 1f;
        [SerializeField] private bool autoUpdate = true;
        [SerializeField] private bool enforceUniformScale = true;

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private int _lastBuildHash;

        private void OnEnable()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();

            EnsureUniformScale();

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "WorldGenDebugSphere" };
                _meshFilter.sharedMesh = _mesh;
            }

            RebuildMesh();
        }

        private void OnValidate()
        {
            longitudeSegments = math.max(8, longitudeSegments);
            latitudeSegments = math.max(4, latitudeSegments);
            _lastBuildHash = 0;
            EnsureUniformScale();
        }

        private void Update()
        {
            EnsureUniformScale();

            if (!autoUpdate || visualizer == null)
                return;

            int currentHash = GetBuildHash();
            if (currentHash != _lastBuildHash)
            {
                RebuildMesh();
            }
        }

        public void RebuildMesh()
        {
            if (visualizer == null)
                return;

            if (visualizer.GetHeightMapTexture() == null)
                visualizer.RegenerateHeightMap();

            var config = visualizer.GetConfig();
            int vertsX = longitudeSegments + 1;
            int vertsY = latitudeSegments + 1;
            int vertexCount = vertsX * vertsY;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];

            HeightMapGenerator2D.SampleHeightMapBurst(
                vertsX,
                vertsY,
                config,
                Allocator.TempJob,
                out NativeArray<float> elevations,
                out NativeArray<VoxelMaterial> vertexMaterials
            );

            for (int y = 0; y <= latitudeSegments; y++)
            {
                float v = y / (float)latitudeSegments;
                float latitude = Mathf.Lerp(90f, -90f, v);

                for (int x = 0; x <= longitudeSegments; x++)
                {
                    float u = x / (float)longitudeSegments;
                    float longitude = Mathf.Lerp(-180f, 180f, u);

                    int index = y * vertsX + x;
                    float radius = config.Radius + elevations[index] * heightScale;

                    vertices[index] = SphericalToCartesian(latitude, longitude, radius);
                    uvs[index] = new Vector2(u, v);
                }
            }

            int materialCount = Enum.GetValues(typeof(VoxelMaterial)).Length;
            List<int>[] submeshTriangles = new List<int>[materialCount];
            for (int i = 0; i < materialCount; i++)
                submeshTriangles[i] = new List<int>();

            for (int y = 0; y < latitudeSegments; y++)
            {
                int row = y * vertsX;
                int nextRow = (y + 1) * vertsX;

                for (int x = 0; x < longitudeSegments; x++)
                {
                    int a = row + x;
                    int b = row + x + 1;
                    int c = nextRow + x;
                    int d = nextRow + x + 1;

                    // Outward-facing winding for Unity front faces.
                    AddTriangleToMaterialSubmesh(submeshTriangles, vertexMaterials, a, b, c);
                    AddTriangleToMaterialSubmesh(submeshTriangles, vertexMaterials, b, d, c);
                }
            }

            _mesh.Clear();
            _mesh.indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            _mesh.vertices = vertices;
            _mesh.uv = uvs;
            _mesh.subMeshCount = materialCount;

            for (int i = 0; i < materialCount; i++)
            {
                _mesh.SetTriangles(submeshTriangles[i], i);
            }

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _meshFilter.sharedMesh = _mesh;

            elevations.Dispose();
            vertexMaterials.Dispose();

            ApplyMaterials(materialCount);
            _lastBuildHash = GetBuildHash();
        }

        private void ApplyMaterials(int materialCount)
        {
            var db = visualizer.GetMaterialDatabase();
            if (db == null)
                return;

            Material[] materials = new Material[materialCount];
            for (int i = 0; i < materialCount; i++)
            {
                materials[i] = db.GetMaterial((VoxelMaterial)i);
            }

            _meshRenderer.sharedMaterials = materials;
        }

        private static Vector3 SphericalToCartesian(float latitudeDeg, float longitudeDeg, float radius)
        {
            float latRad = latitudeDeg * Mathf.Deg2Rad;
            float lonRad = longitudeDeg * Mathf.Deg2Rad;
            float cosLat = Mathf.Cos(latRad);

            return new Vector3(
                radius * cosLat * Mathf.Cos(lonRad),
                radius * Mathf.Sin(latRad),
                radius * cosLat * Mathf.Sin(lonRad)
            );
        }

        private static void AddTriangleToMaterialSubmesh(List<int>[] submeshTriangles, NativeArray<VoxelMaterial> vertexMaterials, int a, int b, int c)
        {
            int submesh = (int)GetDominantMaterial(vertexMaterials[a], vertexMaterials[b], vertexMaterials[c]);
            submeshTriangles[submesh].Add(a);
            submeshTriangles[submesh].Add(b);
            submeshTriangles[submesh].Add(c);
        }

        private static VoxelMaterial GetDominantMaterial(VoxelMaterial a, VoxelMaterial b, VoxelMaterial c)
        {
            if (a == b || a == c)
                return a;
            if (b == c)
                return b;
            return a;
        }

        private int GetBuildHash()
        {
            Texture2D heightMap = visualizer != null ? visualizer.GetHeightMapTexture() : null;
            int textureHash = heightMap != null ? heightMap.GetInstanceID() : 0;
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + textureHash;
                hash = hash * 31 + longitudeSegments;
                hash = hash * 31 + latitudeSegments;
                hash = hash * 31 + Mathf.RoundToInt(heightScale * 1000f);
                return hash;
            }
        }

        private void EnsureUniformScale()
        {
            if (!enforceUniformScale)
                return;

            Vector3 s = transform.localScale;
            float uniform = (math.abs(s.x) + math.abs(s.y) + math.abs(s.z)) / 3f;
            if (uniform <= 1e-5f)
                uniform = 1f;

            Vector3 target = new Vector3(uniform, uniform, uniform);
            if (transform.localScale != target)
                transform.localScale = target;
        }
    }
}


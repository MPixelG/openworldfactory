using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using BurstMeshGeneratorJob2 = _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core.BurstMeshGeneratorJob2;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{

    public static class BurstMeshGenerator
    {
        public const float IsoLevel = 0.5f;
        
        [ReadOnly] private static Tables _tables = new();

        /// <summary>
        /// generates the mesh data for given density field.
        /// </summary>
        /// <param name="field">the density field used for generating the mesh</param>
        /// <param name="cellSize">the size of one cell in world units</param>
        /// <returns>the mesh data of that region</returns>
        public static MeshData GenerateMesh(FieldData field, float cellSize)
        {
            _tables.EnsureInitialized();
            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length;
            BurstMeshGeneratorJob2 job = new BurstMeshGeneratorJob2()
            {
                IsoLevel = 0.5f,
                Triangles = new NativeList<Triangle>(Allocator.Persistent),
                Normals = new NativeList<float3>(Allocator.Persistent),
                Vertices = new NativeList<float3>(Allocator.Persistent),
                VertexMap = new NativeHashMap<VertexKey, int>(50000, Allocator.Persistent),

                Field = field,
                CellSize = cellSize,
                MaterialCount = materialCount,
                RegularCellClass = _tables.RegularCellClass,
                RegularCellData = _tables.RegularCellData,
                RegularVertexData = _tables.RegularVertexData,
            };

            JobHandle handle = job.Schedule();
            handle.Complete();

            MeshData meshData = new MeshData(
                job.Vertices,
                job.Normals,
                job.Triangles
            );


            job.VertexMap.Dispose();
            return meshData;
        }

        /// <summary>
        /// generates the mesh data for given density field using the marching cubes algorithm.
        /// </summary>
        /// <param name="densityJobHandle">the density job that must finish before meshing starts</param>
        /// <param name="field">the density field used for generating the mesh</param>
        /// <param name="cellSize">the size of one cell in world units</param>
        /// <param name="triangles">the generated triangle list</param>
        /// <param name="vertices">the generated vertex list</param>
        /// <param name="normals">the generated normal list</param>
        /// <param name="vertexMap">the generated vertex deduplication map</param>
        /// <returns>the mesh data of that region</returns>
        public static JobHandle ScheduleGenerateMarchingCubesMesh(JobHandle densityJobHandle, FieldData field, float cellSize, out NativeList<Triangle> triangles,  out NativeList<float3> vertices, out NativeList<float3> normals, out NativeHashMap<VertexKey, int> vertexMap)
        {
            _tables.EnsureInitialized();
            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length;
            BurstMeshGeneratorJob2 job = new BurstMeshGeneratorJob2()
            {
                IsoLevel = 0.5f,
                Triangles = new NativeList<Triangle>(Allocator.Persistent),
                Normals = new NativeList<float3>(Allocator.Persistent),
                Vertices = new NativeList<float3>(Allocator.Persistent),
                VertexMap = new NativeHashMap<VertexKey, int>(100, Allocator.Persistent),

                Field = field,
                CellSize = cellSize,
                
                MaterialCount = materialCount,
                RegularCellClass = _tables.RegularCellClass,
                RegularCellData = _tables.RegularCellData,
                RegularVertexData = _tables.RegularVertexData,
            };
            
            triangles = job.Triangles;
            vertices = job.Vertices;
            normals = job.Normals;
            vertexMap = job.VertexMap;

            JobHandle handle = job.Schedule(dependsOn: densityJobHandle);
            return handle;
        }
        
        
        /*
        /// <summary>
        /// generates a transition mesh between two given density field using the Transvoxel algorithm //TODO
        /// </summary>
        /// <param name="densityJobHandle">the density job that must finish before meshing starts</param>
        /// <param name="field">the density field used for generating the mesh</param>
        /// <param name="cellSize">the size of one cell in world units</param>
        /// <param name="triangles">the generated triangle list</param>
        /// <param name="vertices">the generated vertex list</param>
        /// <param name="normals">the generated normal list</param>
        /// <param name="vertexMap">the generated vertex deduplication map</param>
        /// <returns>the mesh data of that region</returns>
        public static JobHandle ScheduleGenerateTransvoxelMesh(JobHandle densityJobHandle, FieldData field, float cellSize, out NativeList<Triangle> triangles, out NativeList<float3> vertices, out NativeList<float3> normals, out NativeHashMap<int, int> vertexMap)
        {
            _tables.EnsureInitialized();
            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length; //todo
            BurstMeshGeneratorJob2 job = new BurstMeshGeneratorJob2()
            {
                IsoLevel = 0.5f,
                Triangles = new NativeList<Triangle>(Allocator.Persistent),
                Normals = new NativeList<float3>(Allocator.Persistent),
                Vertices = new NativeList<float3>(Allocator.Persistent),
                VertexMap = new NativeHashMap<int, int>(5000000, Allocator.Persistent),

                Field = field,
                CellSize = cellSize,
                
                MaterialCount = materialCount,
                
                RegularCellClass = _tables.RegularCellClass,
                RegularCellData = _tables.RegularCellData,
                RegularVertexData = _tables.RegularVertexData,
            };

            triangles = job.Triangles;
            vertices = job.Vertices;
            normals = job.Normals;
            vertexMap = job.VertexMap;

            JobHandle handle = job.Schedule(dependsOn: densityJobHandle);
            return handle;
        }*/
    }
}

using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{

    public static class BurstMeshGenerator
    {
        public const float IsoLevel = 0.5f;

        /// <summary>
        /// generates the mesh data for given density field.
        /// </summary>
        /// <param name="field">the density field used for generating the mesh</param>
        /// <returns>the mesh data of that region</returns>
        public static MeshData GenerateMesh(FieldData field, float cellSize)
        {
            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length;
            BurstMeshGeneratorJob job = new BurstMeshGeneratorJob()
            {
                IsoLevel = 0.5f,
                Triangles = new NativeList<Triangle>(Allocator.Persistent),
                Normals = new NativeList<float3>(Allocator.Persistent),
                Vertices = new NativeList<float3>(Allocator.Persistent),
                VertexMap = new NativeHashMap<VertexKey, int>(50000, Allocator.Persistent),

                Field = field,
                CellSize = cellSize,
                MaterialCount = materialCount,
                RegularCellClass = Tables.RegularCellClass,
                RegularCellData = Tables.RegularCellData,
                RegularVertexData = Tables.RegularVertexData,
                TransitionCellClass = Tables.TransitionCellClass,
                TransitionCornerData = Tables.TransitionCornerData,
                TransitionVertexData = Tables.TransitionVertexData,
                TransitionCellData = Tables.TransitionCellData
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
        /// <param name="field">the density field used for generating the mesh</param>
        /// <returns>the mesh data of that region</returns>
        public static JobHandle ScheduleGenerateMarchingCubesMesh(JobHandle densityJobHandle, FieldData field, float cellSize, out NativeList<Triangle> triangles, out NativeList<float3> Vertices, out NativeList<float3> Normals, out NativeHashMap<VertexKey, int> VertexMap)
        {
            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length;
            BurstMeshGeneratorJob job = new BurstMeshGeneratorJob()
            {
                IsoLevel = 0.5f,
                Triangles = new NativeList<Triangle>(Allocator.Persistent),
                Normals = new NativeList<float3>(Allocator.Persistent),
                Vertices = new NativeList<float3>(Allocator.Persistent),
                VertexMap = new NativeHashMap<VertexKey, int>(500000, Allocator.Persistent),

                Field = field,
                CellSize = cellSize,
                
                MaterialCount = materialCount,
                RegularCellClass = Tables.RegularCellClass,
                RegularCellData = Tables.RegularCellData,
                RegularVertexData = Tables.RegularVertexData,
                TransitionCellClass = Tables.TransitionCellClass,
                TransitionCornerData = Tables.TransitionCornerData,
                TransitionVertexData = Tables.TransitionVertexData,
                TransitionCellData = Tables.TransitionCellData
            };

            triangles = job.Triangles;
            Vertices = job.Vertices;
            Normals = job.Normals;
            VertexMap = job.VertexMap;

            JobHandle handle = job.Schedule(dependsOn: densityJobHandle);
            return handle;
        }
        
        
        /// <summary>
        /// generates a transition mesh between two given density field using the Transvoxel algorithm
        /// </summary>
        /// <param name="field">the density field used for generating the mesh</param>
        /// <returns>the mesh data of that region</returns>
        public static JobHandle ScheduleGenerateTransvoxelMesh(JobHandle densityJobHandle, FieldData field, float cellSize, out NativeList<Triangle> triangles, out NativeList<float3> Vertices, out NativeList<float3> Normals, out NativeHashMap<VertexKey, int> VertexMap)
        {
            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length; //todo
            BurstMeshGeneratorJob job = new BurstMeshGeneratorJob()
            {
                IsoLevel = 0.5f,
                Triangles = new NativeList<Triangle>(Allocator.Persistent),
                Normals = new NativeList<float3>(Allocator.Persistent),
                Vertices = new NativeList<float3>(Allocator.Persistent),
                VertexMap = new NativeHashMap<VertexKey, int>(500000, Allocator.Persistent),

                Field = field,
                CellSize = cellSize,
                
                MaterialCount = materialCount,
                
                RegularCellClass = Tables.RegularCellClass,
                RegularCellData = Tables.RegularCellData,
                RegularVertexData = Tables.RegularVertexData,
                TransitionCellClass = Tables.TransitionCellClass,
                TransitionCornerData = Tables.TransitionCornerData,
                TransitionVertexData = Tables.TransitionVertexData,
                TransitionCellData = Tables.TransitionCellData
            };

            triangles = job.Triangles;
            Vertices = job.Vertices;
            Normals = job.Normals;
            VertexMap = job.VertexMap;

            JobHandle handle = job.Schedule(dependsOn: densityJobHandle);
            return handle;
        }
    }
}

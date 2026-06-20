using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
     
    public static class BurstMeshGenerator
    {
        public const float IsoLevel = 0.5f;
        [ReadOnly]
        private static readonly MarchingCubesTables Tables = new();

        /// <summary>
        /// generates the mesh data for given density field.
        /// </summary>
        /// <param name="densityField">the density field used for generating the mesh</param>
        /// <returns>the mesh data of that region</returns>
        public static MeshData GenerateMesh(DensityFieldData densityField)
        {
            Core.BurstMeshGeneratorJob job = new Core.BurstMeshGeneratorJob()
            {
                IsoLevel = 0.5f,
                Indices = new NativeList<int>(Allocator.Persistent),
                Normals = new NativeList<float3>(Allocator.Persistent),
                Vertices = new NativeList<float3>(Allocator.Persistent),
                VertexMap = new NativeHashMap<VertexKey, int>(50000, Allocator.TempJob),
                DensityField = densityField,
                
                EdgeTable = Tables.EdgeTable,
                TriTable = Tables.TriTable,
            };
            
            JobHandle handle = job.Schedule();
            handle.Complete();

            MeshData meshData = new MeshData(
                job.Vertices,
                job.Normals,
                job.Indices
            );

            
            job.VertexMap.Dispose();
            return meshData;
        }
    }
}
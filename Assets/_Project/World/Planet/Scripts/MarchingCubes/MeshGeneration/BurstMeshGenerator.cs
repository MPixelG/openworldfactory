using System.Collections.Generic;
using _Project.World.Planet.Scripts.MarchingCubes.Core;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.BurstMeshGeneration
{
    public static class BurstMeshGenerator
    {
        [ReadOnly]
        private static readonly MarchingCubesTables Tables = new();

        /// <summary>
        /// generates the mesh data for given density field.
        /// </summary>
        /// <param name="densityField">the density field used for generating the mesh</param>
        /// <returns>the mesh data of that region</returns>
        public static MeshData GenerateMesh(DensityFieldData densityField)
        {
            MeshGeneration.Core.BurstMeshGeneratorJob job = new MeshGeneration.Core.BurstMeshGeneratorJob()
            {
                IsoLevel = 0.5f,
                Indices = new NativeList<int>(Allocator.TempJob),
                Normals = new NativeList<float3>(Allocator.TempJob),
                Vertices = new NativeList<float3>(Allocator.TempJob),
                VertexMap = new NativeHashMap<VertexKey, int>(50000, Allocator.TempJob),
                DensityField = densityField,
                
                EdgeTable = Tables.EdgeTable,
                TriTable = Tables.TriTable,
            };
            
            JobHandle handle = job.Schedule();
            handle.Complete();
            // ReSharper disable All (otherwise ReSharper gives a hint that you can convert the for loop but that crashes the program)
            
            var v = new List<float3>(job.Vertices.Length); // todo check if storing the native lists in the mesh data and converting them while converting them to vectors is faster
            
            foreach (var t in job.Vertices)
                v.Add(t);

            var n = new List<float3>(job.Normals.Length);
            
            foreach (var t in job.Normals)
                n.Add(t);
            var i = new List<int>(job.Indices.Length);
            foreach (var t in job.Indices)
                i.Add(t);

            // ReSharper restore All (re-activate ReSharper)

            MeshData meshData = new MeshData(
                v,
                n,
                i
            );

            
            job.VertexMap.Dispose();
            job.Vertices.Dispose();
            job.Normals.Dispose();
            job.Indices.Dispose();
            
            
            return meshData;
        }
    }
}
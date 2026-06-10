using System.Collections.Generic;
using System.Linq;
using _Project.World.Planet.Scripts.MarchingCubes.Core;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes.BurstMeshGeneration
{
    public class BurstMeshGenerator
    {
        private readonly NativeArray<int> _edgeTable;
        private readonly NativeArray<int> _triTable;

        public BurstMeshGenerator()
        {
            _edgeTable = new NativeArray<int>(McTables.EdgeTable, Allocator.Persistent);

            int rows = McTables.TriTable.GetLength(0);
            int cols = McTables.TriTable.GetLength(1);

            _triTable = new NativeArray<int>(rows * cols, Allocator.Persistent);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    _triTable[i * cols + j] = McTables.TriTable[i, j];
                }
            }
        }

        public MeshData GenerateBurstMesh(DensityFieldData densityField)
        {
            BurstMeshGeneratorJob job = new BurstMeshGeneratorJob()
            {
                IsoLevel = 0.5f,
                Indices = new NativeList<int>(Allocator.TempJob),
                Normals = new NativeList<float3>(Allocator.TempJob),
                Vertices = new NativeList<float3>(Allocator.TempJob),
                VertexMap = new NativeHashMap<VertexKey, int>(50000, Allocator.TempJob),
                DensityField = densityField,
                
                EdgeTable = _edgeTable,
                TriTable = _triTable,
            };
            
            JobHandle handle = job.Schedule();
            handle.Complete();
            
            var v = new List<float3>(job.Vertices.Length); // todo check if storing the native lists in the mesh data and converting them while converting them to vectors is faster
            for (int iv = 0; iv < job.Vertices.Length; iv++)
                v.Add(job.Vertices[iv]);
            
            var n = new List<float3>(job.Normals.Length);
            for (int iN = 0; iN < job.Normals.Length; iN++)
                n.Add(job.Normals[iN]);
            
            var i = new List<int>(job.Indices.Length);
            for (int iI = 0; iI < job.Indices.Length; iI++)
                i.Add(job.Indices[iI]);
            


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
using System.Collections.Generic;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public static class UnityMeshBuilder
    {
        // this function actually builds the mesh using the given triangle. this also contains calculating the indices, normals and bounds of the mesh.
        public static Mesh Build(MeshData data)
        {
            return Build(data.Vertices, data.Normals, data.Triangles);
        }
        
        public static Mesh Build(NativeList<float3> verticesNative, NativeList<float3> normalsNative, NativeList<Triangle> trianglesNative)
        {
            Mesh mesh = new()
            {
                indexFormat = IndexFormat.UInt32
            };
            
            List<Vector3> vertices = new List<Vector3>(verticesNative.Length);
            List<Vector3> normals = new List<Vector3>(normalsNative.Length);

            int materialCount = System.Enum.GetValues(typeof(VoxelMaterial)).Length;
            List<int>[] indices = new List<int>[materialCount];
            
            for(int i=0; i<materialCount; i++)
                indices[i] = new List<int>();
            
            // ReSharper disable All (otherwise ReSharper gives a hint that you can convert the for loop but that crashes the program)
            
            foreach(float3 vertex in verticesNative)
            {
                vertices.Add(new Vector3(vertex.x, vertex.y, vertex.z));
            }
            
            foreach(float3 normal in normalsNative)
            {
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
            }

            foreach (Triangle triangle in trianglesNative)
            {
                indices[(int) triangle.Material].Add(triangle.A); //todo cleanup
                indices[(int) triangle.Material].Add(triangle.B);
                indices[(int) triangle.Material].Add(triangle.C);
            }
            
            // ReSharper restore All (re-activate ReSharper)


            mesh.SetVertices(vertices); // apply the vertices to the unity mesh. we have to convert the float3 vertices to Vector3 for this.
            mesh.SetNormals(normals); // same for the normals.

            mesh.subMeshCount = materialCount;
            
            for (int i = 0; i < materialCount; i++)
            {
                if (indices[i].Count == 0) continue; 
                mesh.SetTriangles(indices[i], i);
            }
            // the indices are just a list of ints so we can pass that directly. the submesh of 0 indicates that we only have one material for this mesh, so all triangles belong to the same submesh.
            
            
            mesh.RecalculateBounds(); // and finally we recalculate the bounds of that mesh
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}

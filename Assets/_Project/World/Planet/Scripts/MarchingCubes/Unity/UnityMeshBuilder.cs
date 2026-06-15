using System.Collections.Generic;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes.Unity
{
    public static class UnityMeshBuilder
    {
        // this function actually builds the mesh using the given triangle. this also contains calculating the indices, normals and bounds of the mesh.
        public static Mesh Build(MeshData data)
        {
            Mesh mesh = new()
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32, // this allows us to have more than 65535 vertices in the mesh, which is important for large chunks.
            };
            
            List<Vector3> vertices = new List<Vector3>(data.Vertices.Length);
            List<Vector3> normals = new List<Vector3>(data.Normals.Length);
            List<int> indices = new List<int>(data.Indices.Length);
            
            // ReSharper disable All (otherwise ReSharper gives a hint that you can convert the for loop but that crashes the program)
            
            foreach(float3 vertex in data.Vertices)
            {
                vertices.Add(new Vector3(vertex.x, vertex.y, vertex.z));
            }
            
            foreach(float3 normal in data.Normals)
            {
                normals.Add(new Vector3(normal.x, normal.y, normal.z));
            }

            foreach (int index in data.Indices)
            {
                indices.Add(index);
            }
            
            // ReSharper restore All (re-activate ReSharper)


            mesh.SetVertices(vertices); // apply the vertices to the unity mesh. we have to convert the float3 vertices to Vector3 for this.
            mesh.SetNormals(normals); // same for the normals.
            mesh.SetTriangles(indices, 0); // the indices are just a list of ints so we can pass that directly. the submesh of 0 indicates that we only have one material for this mesh, so all triangles belong to the same submesh.


            mesh.RecalculateBounds(); // and finally we recalculate the bounds of that mesh

            return mesh;
        }
    }
}

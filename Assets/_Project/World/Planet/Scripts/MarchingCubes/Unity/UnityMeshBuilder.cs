using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
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


            mesh.SetVertices(data.Vertices.ConvertAll(v => new Vector3(v.x, v.y, v.z))); // apply the vertices to the unity mesh. we have to convert the float3 vertices to Vector3 for this.
            mesh.SetNormals(data.Normals.ConvertAll(v => new Vector3(v.x, v.y, v.z))); // same for the normals.
            mesh.SetTriangles(data.Indices, 0); // the indices are just a list of ints so we can pass that directly. the submesh of 0 indicates that we only have one material for this mesh, so all triangles belong to the same submesh.


            mesh.RecalculateBounds(); // and finally we recalculate the bounds of that mesh

            return mesh;
        }
    }
}

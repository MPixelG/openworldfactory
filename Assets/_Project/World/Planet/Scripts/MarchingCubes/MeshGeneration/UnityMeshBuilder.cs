using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
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


            mesh.SetVertices(data.Vertices.ConvertAll(v => new Vector3(v.x, v.y, v.z)));
            mesh.SetNormals(data.Normals.ConvertAll(v => new Vector3(v.x, v.y, v.z)));
            mesh.SetTriangles(data.Indices, 0);


            mesh.RecalculateBounds();

            return mesh;
        }
    }
}

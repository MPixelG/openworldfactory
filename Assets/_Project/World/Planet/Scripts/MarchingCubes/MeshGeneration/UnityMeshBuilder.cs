using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public static class UnityMeshBuilder
    {
        
        /*public static Mesh GenerateMesh(MarchingCubesGrid grid, float isoLevel)
        {
            
            List<Triangle> allTriangles = new List<Triangle>(); //will contain the triangles

            grid.ForEach((pos, _) => // is called for every density in the 3d array
            {
                var tris = MarchingCubesMesher.GenerateAt(pos, grid, isoLevel); // generate the triangles using the marching cubes algorithm
                allTriangles.AddRange(tris); //add all 
            });
            
            Mesh mesh = BuildMesh(allTriangles.ToArray()); // build the mesh from the triangles

            return mesh;
        }*/
        
        
        // this function actually builds the mesh using the given triangle. this also contains calculating the indices, normals and bounds of the mesh.
        public static Mesh Build(MeshData data)
        {
            Mesh mesh = new()
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32, // this allows us to have more than 65535 vertices in the mesh, which is important for large chunks.
            };


            mesh.SetVertices(data.GetVertexVectors());
            mesh.SetNormals(data.GetNormalVectors());
            mesh.SetTriangles(data.Indices, 0);

            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
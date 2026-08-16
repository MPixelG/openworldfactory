using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    /// <summary>
    /// a small class that stores vertices, normals and indices. 
    /// </summary>
    public class MeshData
    {
        public NativeList<float3> Vertices;
        public NativeList<float3> Normals;
        public NativeList<Triangle> Triangles;

        public MeshData(NativeList<float3> vertices, NativeList<float3> normals, NativeList<Triangle> triangles)
        {
            Vertices = vertices;
            Normals = normals;
            Triangles = triangles;
        }

        public void Dispose()
        {
            Vertices.Dispose();
            Normals.Dispose();
            Triangles.Dispose();
        }
    }
}
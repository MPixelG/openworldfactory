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
        public NativeList<int> Indices;

        public MeshData(NativeList<float3> vertices, NativeList<float3> normals, NativeList<int> indices)
        {
            Vertices = vertices;
            Normals = normals;
            Indices = indices;
        }

        public void Dispose()
        {
            Vertices.Dispose();
            Normals.Dispose();
            Indices.Dispose();
        }
    }
}
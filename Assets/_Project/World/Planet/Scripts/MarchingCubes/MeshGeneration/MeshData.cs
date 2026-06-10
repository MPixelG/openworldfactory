using System.Collections.Generic;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    /// <summary>
    /// a small class that stores vertices, normals and indices. 
    /// </summary>
    public class MeshData
    {
        public readonly List<float3> Vertices;
        public readonly List<float3> Normals;
        public readonly List<int> Indices;

        public MeshData(List<float3> vertices, List<float3> normals, List<int> indices)
        {
            Vertices = vertices;
            Normals = normals;
            Indices = indices;
        }
    }
}
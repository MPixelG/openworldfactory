using System.Collections.Generic;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public class MeshData
    {
        public readonly List<float3> Vertices = new();
        public readonly List<float3> Normals = new();
        public readonly List<int> Indices = new();
    }
}
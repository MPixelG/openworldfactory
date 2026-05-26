using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public class MeshData
    {
        public readonly List<float3> Vertices = new();
        public readonly List<float3> Normals = new();
        public readonly List<int> Indices = new();
        
        public List<Vector3> GetVertexVectors() => Vertices.ConvertAll(v => new Vector3(v.x, v.y, v.z));
        public List<Vector3> GetNormalVectors() => Normals.ConvertAll(v => new Vector3(v.x, v.y, v.z));
    }
}
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.v2.Data
{
    public struct ChunkPayload
    {
        public DensityFieldData DensityField;
        public NativeArray<float3> Vertices;
        public NativeArray<float3> Normals;
        public NativeArray<int> Indices;
    }
}
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.v2.Data
{
    public struct ChunkPayload
    {
        public DensityFieldData DensityField;
        public NativeList<float3> Vertices;
        public NativeList<float3> Normals;
        public NativeList<int> Indices;

        /// <summary>
        /// Dispose all native collections to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            if (DensityField.Densities.IsCreated)
                DensityField.Densities.Dispose();
            if (Vertices.IsCreated)
                Vertices.Dispose();
            if (Normals.IsCreated)
                Normals.Dispose();
            if (Indices.IsCreated)
                Indices.Dispose();
        }
    }
}
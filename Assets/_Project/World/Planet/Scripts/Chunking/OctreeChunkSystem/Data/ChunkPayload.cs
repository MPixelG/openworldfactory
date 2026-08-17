using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Data
{
    public struct ChunkPayload
    {
        public FieldData Field;
        public NativeList<float3> Vertices;
        public NativeList<float3> Normals;
        public NativeList<Triangle> Triangles;

        /// <summary>
        /// Dispose all native collections to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            if (Field.Fields.IsCreated)
                Field.Fields.Dispose();
            if (Vertices.IsCreated)
                Vertices.Dispose();
            if (Normals.IsCreated)
                Normals.Dispose();
            if (Triangles.IsCreated)
                Triangles.Dispose();
        }
    }
}
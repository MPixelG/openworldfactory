using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    [BurstCompile]
    public partial struct BurstMeshGeneratorJob : IJob
    {
        [ReadOnly] public DensityFieldData DensityField;

        public NativeList<float3> Vertices;
        public NativeList<float3> Normals;
        public NativeList<int> Indices;

        [DeallocateOnJobCompletion]
        public NativeHashMap<VertexKey, int> VertexMap; // this is used for index deduplication.
        // since every triangle has 3 vertices and many triangles share vertices with each other, we want to avoid adding the same vertex multiple times to the vertices list.
        // that would be a waste of memory and also cause visual artifacts. so we use this dictionary to check if we already added a vertex and if so we just reuse its index instead of adding it again.

        public float IsoLevel;

        [ReadOnly] public NativeArray<int> EdgeTable;
        [ReadOnly] public NativeArray<int> TriTable;

        public void Execute()
        {
            int size = DensityField.Size - 2;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        GenerateAt((new int3(x, y, z)),
                            DensityField); // generate the mesh for every grid cell and use the mesh builder to add it all to one large mesh
                    }
                }
            }

            NormalizeNormals();
        }
    }
}
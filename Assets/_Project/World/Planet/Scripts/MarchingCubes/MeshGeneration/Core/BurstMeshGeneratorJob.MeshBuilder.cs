using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public partial struct BurstMeshGeneratorJob
    {
        /// <summary>
        /// this adds a triangle to the mesh and calculates its indices and normals.
        /// this way you can just call this method for every triangle you want to add and it will take care of the rest.
        /// </summary>
        private void AddTriangle(
            float3 a, VertexKey ka,
            float3 b, VertexKey kb,
            float3 c, VertexKey kc)
        {
            if (!math.all(math.isfinite(a)) ||
                !math.all(math.isfinite(b)) ||
                !math.all(math.isfinite(c)))
                return;

            int i0 = GetOrAddVertex(ka, a);
            int i1 = GetOrAddVertex(kb, b);
            int i2 = GetOrAddVertex(kc, c);

            Indices.Add(i0);
            Indices.Add(i1);
            Indices.Add(i2);

            float3 normal = math.normalize(math.cross(b - a, c - a));

            Normals[i0] += normal;
            Normals[i1] += normal;
            Normals[i2] += normal;
        }

        /// <summary>
        /// checks if the given vertex already exists in the vertices list and if so returns its index,
        /// otherwise it adds it to the list and returns the new index
        /// </summary>
        private int GetOrAddVertex(VertexKey key, float3 v)
        {
            if (VertexMap.TryGetValue(key, out int index))
                return index;

            index = Vertices.Length;

            Vertices.Add(v);
            Normals.Add(float3.zero);

            VertexMap.Add(key, index);

            return index;
        }

        /// <summary>
        /// this normalizes the normals. that means every normal has a length of exactly 1.
        /// </summary>
        private void NormalizeNormals()
        {
            for (int i = 0; i < Normals.Length; i++)
            {
                float3 n = Normals[i];
                if (!math.all(math.isfinite(n)) || math.lengthsq(n) < 1e-12f)
                {
                    Normals[i] = new float3(0f, 1f, 0f);
                    continue;
                }

                Normals[i] = math.normalize(n);
            }
        }
    }
}
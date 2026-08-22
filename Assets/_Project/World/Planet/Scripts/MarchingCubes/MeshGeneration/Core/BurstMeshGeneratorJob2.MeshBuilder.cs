using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public partial struct BurstMeshGeneratorJob2
    {
        /// <summary>
        /// this adds a triangle to the mesh and calculates its indices and normals.
        /// this way you can just call this method for every triangle you want to add and it will take care of the rest.
        /// </summary>
        private static void AddTriangle(
            ref NativeHashMap<VertexKey, int> vertexMap,
            ref NativeList<float3> vertices,
            ref NativeList<float3> normals,
            ref NativeList<Triangle> triangles,
            float3 a, VertexKey ka,
            float3 b, VertexKey kb,
            float3 c, VertexKey kc,
            VoxelMaterial material)
        {
            if (!math.all(math.isfinite(a)) ||
                !math.all(math.isfinite(b)) ||
                !math.all(math.isfinite(c)))
                return;

            int i0 = GetOrAddVertex(ref vertexMap, ref vertices, ref normals, ka, a);
            int i1 = GetOrAddVertex(ref vertexMap, ref vertices, ref normals, kb, b);
            int i2 = GetOrAddVertex(ref vertexMap, ref vertices, ref normals, kc, c);

            triangles.Add(new Triangle(i0, i1, i2, material));

            float3 normal = math.cross(b - a, c - a);
            normals[i0] += normal;
            normals[i1] += normal;
            normals[i2] += normal;
        }


        /// <summary>
        /// checks if the given vertex already exists in the vertices list and if so returns its index,
        /// otherwise it adds it to the list and returns the new index
        /// </summary>
        private static int GetOrAddVertex(
            ref NativeHashMap<VertexKey, int> vertexMap, 
            ref NativeList<float3> vertices, 
            ref NativeList<float3> normals, 
            VertexKey key, 
            float3 v)
        {
            if (vertexMap.TryGetValue(key, out int index))
            {
                return index; 
            }

            index = vertices.Length;

            vertices.Add(v);
            normals.Add(new float3(0, 0, 0));

            vertexMap.Add(key, index);

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
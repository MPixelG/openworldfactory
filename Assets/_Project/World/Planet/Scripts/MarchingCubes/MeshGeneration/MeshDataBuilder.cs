using System.Collections.Generic;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public class MeshDataBuilder
    {
        private readonly MeshData _meshData = new();

        private readonly Dictionary<int3, int> _vertexMap = new(); // this is used for index deduplication.
                                                                     // since every triangle has 3 vertices and many triangles share vertices with each other, we want to avoid adding the same vertex multiple times to the vertices list.
                                                                     // that would be a waste of memory and also cause visual artifacts. so we use this dictionary to check if we already added a vertex and if so we just reuse its index instead of adding it again.


        public MeshData Build() => _meshData;

        /// <summary>
        /// this adds a triangle to the mesh and calculates its indices and normals. this way you can just call this method for every triangle you want to add and it will take care of the rest.
        /// </summary>
        public void AddTriangle(float3 a, float3 b, float3 c)
        {
            // Skip triangles with invalid or degenerate data to avoid NaN normals.
            if (!math.all(math.isfinite(a)) || !math.all(math.isfinite(b)) || !math.all(math.isfinite(c)))
            {
                return;
            }

            float3 ab = b - a;
            float3 ac = c - a;
            float3 normal = math.cross(ab, ac);
            if (math.lengthsq(normal) < 1e-12f)
            {
                return;
            }

            int i0 = GetOrAddVertex(a);
            int i1 = GetOrAddVertex(b);
            int i2 = GetOrAddVertex(c);

            _meshData.Indices.Add(i0);
            _meshData.Indices.Add(i1);
            _meshData.Indices.Add(i2);

            float area = math.length(normal);
            
            float angleA = AngleBetween(ab, ac);
            float angleB = AngleBetween(c - b, a - b);
            float angleC = AngleBetween(a - c, b - c);

            float3 faceNormal = math.normalize(math.cross(ab, ac));

            _meshData.Normals[i0] += faceNormal * angleA;
            _meshData.Normals[i1] += faceNormal * angleB;
            _meshData.Normals[i2] += faceNormal * angleC;
            
        }
        
        /// <summary>
        /// checks if the given vertex already exists in the vertices list and if so returns its index,
        /// otherwise it adds it to the list and returns the new index
        /// </summary>
        /// <param name="v">the vertex to add or get</param>
        private int GetOrAddVertex(float3 v)
        {
            int3 vertexKey = GetVertexKey(v);
            if (_vertexMap.TryGetValue(vertexKey, out int index))
                return index;
            

            index = _meshData.Vertices.Count;

            _meshData.Vertices.Add(v);
            _meshData.Normals.Add(float3.zero);

            _vertexMap.Add(vertexKey, index);

            return index;
        }

        /// <summary>
        /// this normalizes the normals. that means every normal has a length of exactly 1.
        /// </summary>
        public void NormalizeNormals()
        {
            for (int i = 0; i < _meshData.Normals.Count; i++)
            {
                float3 n = _meshData.Normals[i];
                if (!math.all(math.isfinite(n)) || math.lengthsq(n) < 1e-12f)
                {
                    _meshData.Normals[i] = new float3(0f, 1f, 0f);
                    continue;
                }

                _meshData.Normals[i] = math.normalize(n);
            }
        }
        
        private static float AngleBetween(float3 a, float3 b)
        {
            float len = math.length(a) * math.length(b);

            if (len < 1e-8f)
                return 0f;

            float d = math.clamp(math.dot(a, b) / len, -1f, 1f);

            return math.acos(d);
        }
        
        
        private int3 GetVertexKey(float3 v) => (int3)math.round(v * 10f);
    }
}
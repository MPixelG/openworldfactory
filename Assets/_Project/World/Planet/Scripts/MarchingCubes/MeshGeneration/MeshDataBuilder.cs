using System;
using System.Collections.Generic;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public class MeshDataBuilder
    {
        private readonly MeshData _meshData = new(new List<float3>(), new List<float3>(), new List<int>());

        private readonly Dictionary<VertexKey, int> _vertexMap = new(); // this is used for index deduplication.
                                                                     // since every triangle has 3 vertices and many triangles share vertices with each other, we want to avoid adding the same vertex multiple times to the vertices list.
                                                                     // that would be a waste of memory and also cause visual artifacts. so we use this dictionary to check if we already added a vertex and if so we just reuse its index instead of adding it again.

        private readonly DensityField _densityField; // we need the density field to calculate the normals. we could pass it as a parameter to the AddTriangle method but since we need it for every triangle it would be more efficient to just store it as a field in the builder.

        public MeshData Build() => _meshData;

        public MeshDataBuilder(DensityField densityField)
        {
            _densityField = densityField;
        }

        /// <summary>
        /// this adds a triangle to the mesh and calculates its indices and normals. this way you can just call this method for every triangle you want to add and it will take care of the rest.
        /// </summary>
        public void AddTriangle(
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

            _meshData.Indices.Add(i0);
            _meshData.Indices.Add(i1);
            _meshData.Indices.Add(i2);

            float3 normal = math.normalize(math.cross(b - a, c - a));

            _meshData.Normals[i0] += normal;
            _meshData.Normals[i1] += normal;
            _meshData.Normals[i2] += normal;
        }
        
        /// <summary>
        /// checks if the given vertex already exists in the vertices list and if so returns its index,
        /// otherwise it adds it to the list and returns the new index
        /// </summary>
        /// <param name="v">the vertex to add or get</param>
        private int GetOrAddVertex(VertexKey key, float3 v)
        {
            if (_vertexMap.TryGetValue(key, out int index))
                return index;

            index = _meshData.Vertices.Count;

            _meshData.Vertices.Add(v);
            _meshData.Normals.Add(float3.zero);

            _vertexMap.Add(key, index);

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
        
        /// <summary>
        /// calculates the angle between 2 vectors
        /// </summary>
        /// <param name="a">vector a</param>
        /// <param name="b">vector b</param>
        /// <returns>the angle in radians</returns>
        private static float AngleBetween(float3 a, float3 b)
        {
            float len = math.length(a) * math.length(b); // get the length

            if (len < 1e-8f) // and check if its tiny, if it is we can skip the calculation since something that tiny will result in something even tinier and we can just return 0 to be more efficient 
                return 0f;

            float d = math.clamp(math.dot(a, b) / len, -1f, 1f); // now just calculate the dot product and scale it down by the length.
                                                                 // due to floating point inaccuracies it can happen that the value is slightly above 1 or below -1 so we just clamp it.

            return math.acos(d); // finally we use acos to get the angle in radians
        }
        
        
        private static float3 CalculateSmoothNormal(float3 pos, DensityField densityField)
        {
            // we sample slightly around the position to estimate the gradient
            // this works like a derivative of the density field
            
            float eps = 0.1f;
            

            float dx =
                densityField.DensityAt(pos + new float3(eps,0,0))
                - densityField.DensityAt(pos - new float3(eps,0,0));

            float dy =
                densityField.DensityAt(pos + new float3(0,eps,0))
                - densityField.DensityAt(pos - new float3(0,eps,0));

            float dz =
                densityField.DensityAt(pos + new float3(0,0,eps))
                - densityField.DensityAt(pos - new float3(0,0,eps));

            float3 gradient = new float3(dx, dy, dz);

            // fallback in case gradient is zero
            if (math.lengthsq(gradient) < 1e-12f)
            {
                return new float3(0, 1, 0);
            }

            // negative because density usually increases inward
            return math.normalize(-gradient);
        }
    }
    
    public readonly struct VertexKey : IEquatable<VertexKey>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public readonly byte Edge;

        public VertexKey(int3 cell, byte edge)
        {
            X = cell.x;
            Y = cell.y;
            Z = cell.z;
            Edge = edge;
        }

        public bool Equals(VertexKey other)
            => X == other.X && Y == other.Y && Z == other.Z && Edge == other.Edge;

        public override int GetHashCode()
        {
            return (int)math.hash(new int4(X, Y, Z, Edge));
        }
    }
}
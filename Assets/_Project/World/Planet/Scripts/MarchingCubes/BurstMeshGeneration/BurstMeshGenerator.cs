using _Project.World.Planet.Scripts.MarchingCubes.Core;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.BurstMeshGeneration
{
    [BurstCompile]
    public struct BurstMeshGeneratorJob : IJob
    {
        public DensityFieldData DensityField;

        public NativeList<float3> Vertices;
        public NativeList<float3> Normals;
        public NativeList<int> Indices;

        public NativeHashMap<VertexKey, int> VertexMap;

        public float IsoLevel;
        
        
        // since every triangle has 3 vertices and many triangles share vertices with each other, we want to avoid adding the same vertex multiple times to the vertices list.
        // that would be a waste of memory and also cause visual artifacts. so we use this dictionary to check if we already added a vertex and if so we just reuse its index instead of adding it again.

        
        public void Execute()
        {
            DensityField.Dispose();
            
            
            int size = DensityField.Size-2;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        GenerateAt((new int3(x, y, z)), DensityField); // generate the mesh for every grid cell and use the mesh builder to add it all to one large mesh
                    }
                }
            }
            
            NormalizeNormals();
        }

        private void GenerateAt(int3 pos, DensityFieldData grid)
        {
            int cubeIndex =
                GetCubeIndexAt(pos, grid,
                    IsoLevel); // calculates the cube index at that position. look at the description of that function if you want to know what it does

            if (cubeIndex is 255 or 0) return;

            int
                edgeFlags = McTables
                    .EdgeTable
                        [cubeIndex]; // converts the given corner configuration to a binary number where every bit represents one edge. note that we convert an 8 bit number (one bit for every corner) to a 12 bit number (one bit for every edge). 
            // we take that value from a huge precomputed table that contains the edge configuration for every possible corner configuration. So every bit is 1 if the isosurface cuts through that edge. 


            if (edgeFlags ==
                0) // if the isosurface doesnt cut through any edge, we can skip this cube and return an empty list of triangles
                return;

            Vtx[]
                vertices =
                    new Vtx[12]; // the resulting vertices can be 12 max since there are only 12 edges. we fill the rest of the values with -1.

            // helper vars for the single corners of the cube
            int3 p0 = new int3(pos.x, pos.y, pos.z);
            int3 p1 = new int3(pos.x + 1, pos.y, pos.z);
            int3 p2 = new int3(pos.x + 1, pos.y + 1, pos.z);
            int3 p3 = new int3(pos.x, pos.y + 1, pos.z);
            int3 p4 = new int3(pos.x, pos.y, pos.z + 1);
            int3 p5 = new int3(pos.x + 1, pos.y, pos.z + 1);
            int3 p6 = new int3(pos.x + 1, pos.y + 1, pos.z + 1);
            int3 p7 = new int3(pos.x, pos.y + 1, pos.z + 1);


            // here we check if the given bit is 1. the binary operator & acts as an AND mask, so 0b11011101 & 0b01010011 = 0b01010001. this way we can check every edge separately. c# doesnt let you directly cast ints to bools (1=true, 0=false) so we just compare it to 0 to know if its true or not
            if ((edgeFlags & 1) != 0)
            {
                vertices[0].Pos =
                    VertexInterp(IsoLevel, p0,
                        p1, // so we know now that this edge is used but if we just pass the center between the start and end point of that edge it would be way to blocky. so we just interpolate between the 2 densities of the corresponding values. this way everything is distributed smoothly.
                        grid.DensityAt(p0), grid.DensityAt(p1));

                vertices[0].Key = new VertexKey(pos, 0);
            }

            if ((edgeFlags & 2) != 0)
            {
                vertices[1].Pos =
                    VertexInterp(IsoLevel, p1, p2,
                        grid.DensityAt(p1), grid.DensityAt(p2));

                vertices[1].Key = new VertexKey(pos, 1);
            }

            if ((edgeFlags & 4) != 0)
            {
                vertices[2].Pos =
                    VertexInterp(IsoLevel, p2, p3,
                        grid.DensityAt(p2), grid.DensityAt(p3));

                vertices[2].Key = new VertexKey(pos, 2);
            }

            if ((edgeFlags & 8) != 0)
            {
                vertices[3].Pos =
                    VertexInterp(IsoLevel, p3, p0,
                        grid.DensityAt(p3), grid.DensityAt(p0));

                vertices[3].Key = new VertexKey(pos, 3);
            }

            if ((edgeFlags & 16) != 0)
            {
                vertices[4].Pos =
                    VertexInterp(IsoLevel, p4, p5,
                        grid.DensityAt(p4), grid.DensityAt(p5));

                vertices[4].Key = new VertexKey(pos, 4);
            }

            if ((edgeFlags & 32) != 0)
            {
                vertices[5].Pos =
                    VertexInterp(IsoLevel, p5, p6,
                        grid.DensityAt(p5), grid.DensityAt(p6));

                vertices[5].Key = new VertexKey(pos, 5);
            }

            if ((edgeFlags & 64) != 0)
            {
                vertices[6].Pos =
                    VertexInterp(IsoLevel, p6, p7,
                        grid.DensityAt(p6), grid.DensityAt(p7));

                vertices[6].Key = new VertexKey(pos, 6);
            }

            if ((edgeFlags & 128) != 0)
            {
                vertices[7].Pos =
                    VertexInterp(IsoLevel, p7, p4,
                        grid.DensityAt(p7), grid.DensityAt(p4));

                vertices[7].Key = new VertexKey(pos, 7);
            }

            if ((edgeFlags & 256) != 0)
            {
                vertices[8].Pos =
                    VertexInterp(IsoLevel, p0, p4,
                        grid.DensityAt(p0), grid.DensityAt(p4));

                vertices[8].Key = new VertexKey(pos, 8);
            }

            if ((edgeFlags & 512) != 0)
            {
                vertices[9].Pos =
                    VertexInterp(IsoLevel, p1, p5,
                        grid.DensityAt(p1), grid.DensityAt(p5));

                vertices[9].Key = new VertexKey(pos, 9);
            }

            if ((edgeFlags & 1024) != 0)
            {
                vertices[10].Pos =
                    VertexInterp(IsoLevel, p2, p6,
                        grid.DensityAt(p2), grid.DensityAt(p6));

                vertices[10].Key = new VertexKey(pos, 10);
            }

            if ((edgeFlags & 2048) != 0)
            {
                vertices[11].Pos =
                    VertexInterp(IsoLevel, p3, p7,
                        grid.DensityAt(p3), grid.DensityAt(p7));

                vertices[11].Key = new VertexKey(pos, 11);
            }


            //now we need to convert the vertices to triangles and take just the filled ones

            //so we have the used edges but we dont know how the triangles are supposed to be generated to occupy the given edge configuration. 
            // thats why we have another huge precomputed table that contains the triangle configuration for every possible edge configuration. so we just need to loop through that table until we find a -1 which marks the end of the triangle list for that edge configuration.
            for (int i = 0;
                 McTables.TriTable[cubeIndex, i] != -1;
                 i += 3) // go in steps of 3 (a triangle consists of 3 points) until we find a -1 which marks the end of that triangle list
            {
                int a = McTables.TriTable[cubeIndex, i];
                int b = McTables.TriTable[cubeIndex, i + 1];
                int c = McTables.TriTable[cubeIndex, i + 2];

                AddTriangle(
                    vertices[a].Pos, vertices[a].Key,
                    vertices[b].Pos, vertices[b].Key,
                    vertices[c].Pos, vertices[c].Key
                );
            }
        }

        private static int GetCubeIndexAt(int3 pos, DensityFieldData grid, float isoLevel)
        {
            int cubeIndex = 0;

            if (grid.DensityAt(pos.x, pos.y, pos.z) > isoLevel)
                cubeIndex |=
                    1; // the |= operator sets every bit that is set in the right operand to 1 in the left operand also to 1.
            // so if the left number (cubeIndex) is 0b11001000 and the right one (the mask, lets say 4) is 0b00000100 the result of that operation would be 0b11001100
            if (grid.DensityAt(pos.x + 1, pos.y, pos.z) > isoLevel) cubeIndex |= 2;
            if (grid.DensityAt(pos.x + 1, pos.y + 1, pos.z) > isoLevel) cubeIndex |= 4;
            if (grid.DensityAt(pos.x, pos.y + 1, pos.z) > isoLevel) cubeIndex |= 8;
            if (grid.DensityAt(pos.x, pos.y, pos.z + 1) > isoLevel) cubeIndex |= 16;
            if (grid.DensityAt(pos.x + 1, pos.y, pos.z + 1) > isoLevel) cubeIndex |= 32;
            if (grid.DensityAt(pos.x + 1, pos.y + 1, pos.z + 1) > isoLevel) cubeIndex |= 64;
            if (grid.DensityAt(pos.x, pos.y + 1, pos.z + 1) > isoLevel) cubeIndex |= 128;

            return cubeIndex;
        }


        // lerps (linear interpolates) between 2 given points based on their density values and the iso level
        private static float3 VertexInterp(float isoLevel, int3 p1, int3 p2, float valP1, float valP2)
        {
            float3 p;

            if (math.abs(isoLevel - valP1) < 0.00001)
                return (new float3(p1.x, p1.y, p1.z));
            if (math.abs(isoLevel - valP2) < 0.00001)
                return (new float3(p2.x, p2.y, p2.z));
            if (math.abs(valP1 - valP2) < 0.00001)
                return (new float3(p1.x, p1.y, p1.z));

            float mu = (isoLevel - valP1) / (valP2 - valP1);
            p.x = p1.x + mu * (p2.x - p1.x);
            p.y = p1.y + mu * (p2.y - p1.y);
            p.z = p1.z + mu * (p2.z - p1.z);

            return p;
        }

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

        private struct Vtx
        {
            public float3 Pos;
            public VertexKey Key;
        }
    }
}
using System.Runtime.CompilerServices;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public unsafe partial struct BurstMeshGeneratorJob
    {
        private void GenerateAt(int3 pos, FieldData grid)
        {
            int cubeIndex = GetCubeIndexAt(pos, grid, IsoLevel); // calculates the cube index at that position.
                                                                // look at the description of that function if you want to know what it does

            if (cubeIndex is 255 or 0) return;  // if the isosurface doesnt cut through any edge, we can skip this cube and return an empty list of triangles

            int edgeFlags = EdgeTable[cubeIndex]; // converts the given corner configuration to a binary number
            // where every bit represents one edge. note that we convert an 8 bit
            // number (one bit for every corner) to a 12 bit number (one bit for every edge). 
            // we take that value from a huge precomputed table that contains the edge configuration
            // for every possible corner configuration. So every bit is 1 if the isosurface cuts through that edge. 

            
            // the resulting vertices can be 12 max since there are only 12 edges. we fill the rest of the values with -1.
            Vtx* vertices = stackalloc Vtx[12];

            // helper vars for the single corners of the cube
            int3 p0 = new int3(pos.x, pos.y, pos.z);
            int3 p1 = new int3(pos.x + 1, pos.y, pos.z);
            int3 p2 = new int3(pos.x + 1, pos.y + 1, pos.z);
            int3 p3 = new int3(pos.x, pos.y + 1, pos.z);
            int3 p4 = new int3(pos.x, pos.y, pos.z + 1);
            int3 p5 = new int3(pos.x + 1, pos.y, pos.z + 1);
            int3 p6 = new int3(pos.x + 1, pos.y + 1, pos.z + 1);
            int3 p7 = new int3(pos.x, pos.y + 1, pos.z + 1);


            // here we check if the given bit is 1. the binary operator & acts as an AND mask, so
            // 0b11011101 & 0b01010011 = 0b01010001. this way we can check every edge separately.
            // c# doesnt let you directly cast ints to bools (1=true, 0=false) so we just compare it to 0 to know if its true or not
            if ((edgeFlags & 1) != 0)
            {
                vertices[0].Pos =
                    VertexInterp(IsoLevel, p0,
                        p1, // so we know now that this edge is used but if we just pass the center between the start and end point of that edge it would be way to blocky. so we just interpolate between the 2 densities of the corresponding values. this way everything is distributed smoothly.
                        grid.DensityAt(p0), grid.DensityAt(p1));

                vertices[0].Key = new VertexKey(pos, 0); // also store the key (consisting of the position and edge index) so that we can deduplicate the vertices later
            }

            if ((edgeFlags & 2) != 0) //same goes for every other edge type
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

            
            VoxelMaterial m0 = grid.MaterialAt(p0.x, p0.y, p0.z);
            VoxelMaterial m1 = grid.MaterialAt(p1.x, p1.y, p1.z);
            VoxelMaterial m2 = grid.MaterialAt(p2.x, p2.y, p2.z);
            VoxelMaterial m3 = grid.MaterialAt(p3.x, p3.y, p3.z);
            VoxelMaterial m4 = grid.MaterialAt(p4.x, p4.y, p4.z);
            VoxelMaterial m5 = grid.MaterialAt(p5.x, p5.y, p5.z);
            VoxelMaterial m6 = grid.MaterialAt(p6.x, p6.y, p6.z);
            VoxelMaterial m7 = grid.MaterialAt(p7.x, p7.y, p7.z);

            //now we need to convert the vertices to triangles and take just the filled ones

            //so we have the used edges but we dont know how the triangles are supposed to be generated to occupy the given edge configuration. 
            // thats why we have another huge precomputed table that contains the triangle configuration for every possible edge configuration. so we just need to loop through that table until we find a -1 which marks the end of the triangle list for that edge configuration.
            for (int i = 0;
                 Tri(TriTable, cubeIndex, i) != -1;
                 i += 3) // go in steps of 3 (a triangle consists of 3 points) until we find a -1 which marks the end of that triangle list
            {
                int a = Tri(TriTable, cubeIndex, i);
                int b = Tri(TriTable, cubeIndex, i + 1); 
                int c = Tri(TriTable, cubeIndex, i + 2);
                
                GetEdgeMaterials(a, m0, m1, m2, m3, m4, m5, m6, m7,
                    out VoxelMaterial a0, out VoxelMaterial a1);

                GetEdgeMaterials(b, m0, m1, m2, m3, m4, m5, m6, m7,
                    out VoxelMaterial b0, out VoxelMaterial b1);

                GetEdgeMaterials(c, m0, m1, m2, m3, m4, m5, m6, m7,
                    out VoxelMaterial c0, out VoxelMaterial c1);
                
                VoxelMaterial material = GetDominantMaterial(
                    a0, a1,
                    b0, b1,
                    c0, c1
                );

                AddTriangle(
                    vertices[a].Pos, vertices[a].Key,
                    vertices[b].Pos, vertices[b].Key,
                    vertices[c].Pos, vertices[c].Key,
                    material
                );
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetEdgeMaterials(
            int edge,
            VoxelMaterial m0,
            VoxelMaterial m1,
            VoxelMaterial m2,
            VoxelMaterial m3,
            VoxelMaterial m4,
            VoxelMaterial m5,
            VoxelMaterial m6,
            VoxelMaterial m7,
            out VoxelMaterial a,
            out VoxelMaterial b)
        {
            switch (edge)
            {
                case 0:  a = m0; b = m1; break;
                case 1:  a = m1; b = m2; break;
                case 2:  a = m2; b = m3; break;
                case 3:  a = m3; b = m0; break;

                case 4:  a = m4; b = m5; break;
                case 5:  a = m5; b = m6; break;
                case 6:  a = m6; b = m7; break;
                case 7:  a = m7; b = m4; break;

                case 8:  a = m0; b = m4; break;
                case 9:  a = m1; b = m5; break;
                case 10: a = m2; b = m6; break;
                case 11: a = m3; b = m7; break;

                default:
                    a = VoxelMaterial.Air;
                    b = VoxelMaterial.Air;
                    break;
            }
        }

        private static VoxelMaterial GetDominantMaterial(
            VoxelMaterial a,
            VoxelMaterial b,
            VoxelMaterial c,
            VoxelMaterial d,
            VoxelMaterial e,
            VoxelMaterial f)
        {
            int* counts = stackalloc int[4];

            counts[(int)a]++;
            counts[(int)b]++;
            counts[(int)c]++;
            counts[(int)d]++;
            counts[(int)e]++;
            counts[(int)f]++;

            int max = 0;
            int maxIndex = 0;

            for (int i = 0; i < 4; i++)
            {
                if (counts[i] > max)
                {
                    max = counts[i];
                    maxIndex = i;
                }
            }

            return (VoxelMaterial)maxIndex;
        }
        

        private static int GetCubeIndexAt(int3 pos, FieldData grid, float isoLevel)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Tri(
            NativeArray<int> triTable,
            int cubeIndex,
            int i)
        {
            return triTable[cubeIndex * 16 + i];
        }

        // lerps (linear interpolates) between 2 given points based on their density values and the iso level
        private float3 VertexInterp(float isoLevel, int3 p1, int3 p2, float valP1, float valP2)
        {
            float3 p;

            if (math.abs(isoLevel - valP1) < 0.00001)
                return new float3(p1.x, p1.y, p1.z);
            if (math.abs(isoLevel - valP2) < 0.00001)
                return new float3(p2.x, p2.y, p2.z);
            if (math.abs(valP1 - valP2) < 0.00001)
                return new float3(p1.x, p1.y, p1.z);

            float mu = (isoLevel - valP1) / (valP2 - valP1);
            p.x = p1.x + mu * (p2.x - p1.x);
            p.y = p1.y + mu * (p2.y - p1.y);
            p.z = p1.z + mu * (p2.z - p1.z);

            return p * CellSize;
        }
    }
}
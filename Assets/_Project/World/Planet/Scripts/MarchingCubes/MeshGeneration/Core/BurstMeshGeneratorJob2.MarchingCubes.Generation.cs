using System.Runtime.CompilerServices;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public unsafe partial struct BurstMeshGeneratorJob2
    {
        private void GenerateMarchingCubesCellAt(int3 pos)
        {
            int cubeIndex = GetMcCubeIndexAt(pos, Field, IsoLevel);
            if (cubeIndex is 255 or 0) return;

            int3* corners = stackalloc int3[8]
            {
                new int3(pos.x, pos.y, pos.z),
                new int3(pos.x + 1, pos.y, pos.z),
                new int3(pos.x, pos.y + 1, pos.z),
                new int3(pos.x + 1, pos.y + 1, pos.z),
                new int3(pos.x, pos.y, pos.z + 1),
                new int3(pos.x + 1, pos.y, pos.z + 1),
                new int3(pos.x, pos.y + 1, pos.z + 1),
                new int3(pos.x + 1, pos.y + 1, pos.z + 1)
            };

            byte cellClass = RegularCellClass[cubeIndex];
            RegularCellData cellData = RegularCellData[cellClass];
            int triangleCount = cellData.TriangleCount;
            int vertexCount = cellData.VertexCount;


            if (triangleCount == 0) return;

            Vtx* vertices = stackalloc Vtx[12];


            int vDataOffset = cubeIndex * 12;


            for (int i = 0; i < vertexCount && i < 12; i++)
            {
                ushort vInfo = RegularVertexData[vDataOffset + i];

                byte cornerA = (byte)((vInfo >> 4) & 0x0F);
                byte cornerB = (byte)(vInfo & 0x0F);

                int3 a = corners[cornerA];
                int3 b = corners[cornerB];

                vertices[i].Pos =
                   VertexInterp(IsoLevel, a, b, Field.DensityAt(a), Field.DensityAt(b));
                

                if (math.any(a > b))
                {
                    (a, b) = (b, a);
                }

                vertices[i].Key = new VertexKey(a, b);
            }

            VoxelMaterial m0 = Field.MaterialAt(corners[0]);
            VoxelMaterial m1 = Field.MaterialAt(corners[1]);
            VoxelMaterial m2 = Field.MaterialAt(corners[2]);
            VoxelMaterial m3 = Field.MaterialAt(corners[3]);
            VoxelMaterial m4 = Field.MaterialAt(corners[4]);
            VoxelMaterial m5 = Field.MaterialAt(corners[5]);
            VoxelMaterial m6 = Field.MaterialAt(corners[6]);
            VoxelMaterial m7 = Field.MaterialAt(corners[7]);

            for (int i = 0; i < triangleCount * 3 && i < 150; i += 3)
            {
                byte internalA = cellData.vertexIndex[i];
                byte internalB = cellData.vertexIndex[i + 1];
                byte internalC = cellData.vertexIndex[i + 2];

                if (internalA >= vertexCount ||
                    internalB >= vertexCount ||
                    internalC >= vertexCount)
                {
                    continue;
                }

                ushort vInfoA = RegularVertexData[vDataOffset + internalA];
                ushort vInfoB = RegularVertexData[vDataOffset + internalB];
                ushort vInfoC = RegularVertexData[vDataOffset + internalC];


                GetMcEdgeMaterials(vInfoA, m0, m1, m2, m3, m4, m5, m6, m7, out VoxelMaterial a0, out VoxelMaterial a1);
                GetMcEdgeMaterials(vInfoB, m0, m1, m2, m3, m4, m5, m6, m7, out VoxelMaterial b0, out VoxelMaterial b1);
                GetMcEdgeMaterials(vInfoC, m0, m1, m2, m3, m4, m5, m6, m7, out VoxelMaterial c0, out VoxelMaterial c1);

                VoxelMaterial material = GetDominantMaterial(a0, a1, b0, b1, c0, c1, MaterialCount);

                AddTriangle(
                    ref VertexMap, ref Vertices, ref Normals, ref Triangles,
                    vertices[internalA].Pos, vertices[internalA].Key,
                    vertices[internalC].Pos, vertices[internalC].Key, // a, c, b (if you do a b c the winding order is incorrect and the wrong faces are being culled)
                    vertices[internalB].Pos, vertices[internalB].Key,
                    material
                );
            }
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetMcEdgeMaterials(
            ushort vertexData,
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
            int cornerA = (vertexData >> 4) & 0xF;
            int cornerB = vertexData & 0xF;

            a = GetCornerMaterial(
                cornerA,
                m0, m1, m2, m3,
                m4, m5, m6, m7);

            b = GetCornerMaterial(
                cornerB,
                m0, m1, m2, m3,
                m4, m5, m6, m7);
        }

        private static VoxelMaterial GetCornerMaterial(
            int corner,
            VoxelMaterial m0,
            VoxelMaterial m1,
            VoxelMaterial m2,
            VoxelMaterial m3,
            VoxelMaterial m4,
            VoxelMaterial m5,
            VoxelMaterial m6,
            VoxelMaterial m7)
        {
            return corner switch
            {
                0 => m0,
                1 => m1,
                2 => m2,
                3 => m3,
                4 => m4,
                5 => m5,
                6 => m6,
                7 => m7,
                _ => VoxelMaterial.Air
            };
        }

        /// <summary>
        /// TODO summary
        /// </summary>>
        /// <param name="pos">the grid position of the cube in the given grid</param>
        /// <param name="grid">the field that contains the density data</param>
        /// <param name="isoLevel">the given isoLevel for the algorithm</param>
        /// <returns>an 8-bit index for a unique variation of a cube</returns>
        private static int GetMcCubeIndexAt(int3 pos, FieldData grid, float isoLevel)
        {
            int cubeIndex = 0;

            if (grid.DensityAt(pos.x, pos.y, pos.z) > isoLevel)
                cubeIndex |=
                    1; // the |= operator sets every bit that is set in the right operand to 1 in the left operand also to 1.   
            // so if the left number (cubeIndex) is 0b11001000 and the right one (the mask, lets say 4) is 0b00000100 the result of that operation would be 0b11001100
            if (grid.DensityAt(pos.x + 1, pos.y, pos.z) > isoLevel) cubeIndex |= 2;
            if (grid.DensityAt(pos.x, pos.y + 1, pos.z) > isoLevel) cubeIndex |= 4;
            if (grid.DensityAt(pos.x + 1, pos.y + 1, pos.z) > isoLevel) cubeIndex |= 8;
            if (grid.DensityAt(pos.x, pos.y, pos.z + 1) > isoLevel) cubeIndex |= 16;
            if (grid.DensityAt(pos.x + 1, pos.y, pos.z + 1) > isoLevel) cubeIndex |= 32;
            if (grid.DensityAt(pos.x, pos.y + 1, pos.z + 1) > isoLevel) cubeIndex |= 64;
            if (grid.DensityAt(pos.x + 1, pos.y + 1, pos.z + 1) > isoLevel) cubeIndex |= 128;

            return cubeIndex;
        }
    }
}
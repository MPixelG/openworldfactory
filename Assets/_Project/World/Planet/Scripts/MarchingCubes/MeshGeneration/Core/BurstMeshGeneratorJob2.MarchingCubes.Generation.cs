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
            float d0 = Field.DensityAt(pos);
            float d1 = Field.DensityAt(pos + new int3(1, 0, 0));
            float d2 = Field.DensityAt(pos + new int3(0, 1, 0));
            float d3 = Field.DensityAt(pos + new int3(1, 1, 0));
            float d4 = Field.DensityAt(pos + new int3(0, 0, 1));
            float d5 = Field.DensityAt(pos + new int3(1, 0, 1));
            float d6 = Field.DensityAt(pos + new int3(0, 1, 1));
            float d7 = Field.DensityAt(pos + new int3(1, 1, 1));
            VoxelMaterial m0 = Field.MaterialAt(pos);
            VoxelMaterial m1 = Field.MaterialAt(pos + new int3(1, 0, 0));
            VoxelMaterial m2 = Field.MaterialAt(pos + new int3(0, 1, 0));
            VoxelMaterial m3 = Field.MaterialAt(pos + new int3(1, 1, 0));
            VoxelMaterial m4 = Field.MaterialAt(pos + new int3(0, 0, 1));
            VoxelMaterial m5 = Field.MaterialAt(pos + new int3(1, 0, 1));
            VoxelMaterial m6 = Field.MaterialAt(pos + new int3(0, 1, 1));
            VoxelMaterial m7 = Field.MaterialAt(pos + new int3(1, 1, 1));
            
            int cubeIndex = GetMcCubeIndexAt(pos, d0, d1, d2, d3, d4, d5, d6, d7, IsoLevel);
            if (cubeIndex is 255 or 0) return;

            (int3, float, VoxelMaterial)* corners = stackalloc (int3, float, VoxelMaterial)[8]
            {
                (new int3(pos.x, pos.y, pos.z), d0, m0),
                (new int3(pos.x + 1, pos.y, pos.z), d1, m1),
                (new int3(pos.x, pos.y + 1, pos.z), d2, m2),
                (new int3(pos.x + 1, pos.y + 1, pos.z), d3, m3),
                (new int3(pos.x, pos.y, pos.z + 1), d4, m4),
                (new int3(pos.x + 1, pos.y, pos.z + 1), d5, m5),
                (new int3(pos.x, pos.y + 1, pos.z + 1), d6, m6),
                (new int3(pos.x + 1, pos.y + 1, pos.z + 1), d7, m7)
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

                (int3 a, float da, _) = corners[cornerA];
                (int3 b, float db, _) = corners[cornerB];

                vertices[i].Pos =
                   VertexInterp(IsoLevel, a, b, da, db);
                

                if (math.any(a > b))
                {
                    (a, b) = (b, a);
                }

                vertices[i].Key = new VertexKey(a, b);
            }


            int* counts = stackalloc int[MaterialCount-1];

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


                GetMcEdgeMaterials(vInfoA, ref corners, out VoxelMaterial a0, out VoxelMaterial a1);
                GetMcEdgeMaterials(vInfoB, ref corners, out VoxelMaterial b0, out VoxelMaterial b1);
                GetMcEdgeMaterials(vInfoC, ref corners, out VoxelMaterial c0, out VoxelMaterial c1);

                VoxelMaterial material = GetDominantMaterial(a0, a1, b0, b1, c0, c1, MaterialCount, ref counts);

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
            ref (int3, float, VoxelMaterial)* cornerData,
            out VoxelMaterial a,
            out VoxelMaterial b)
        {
            int cornerA = (vertexData >> 4) & 0xF;
            int cornerB = vertexData & 0xF;

            a = cornerData[cornerA].Item3;

            b = cornerData[cornerB].Item3;
        }

        /// <summary>
        /// TODO summary
        /// </summary>>
        /// <param name="pos">the grid position of the cube in the given grid</param>
        /// <param name="grid">the field that contains the density data</param>
        /// <param name="isoLevel">the given isoLevel for the algorithm</param>
        /// <returns>an 8-bit index for a unique variation of a cube</returns>
        private static int GetMcCubeIndexAt(int3 pos, float d0, float d1, float d2, float d3, float d4, float d5, float d6, float d7,  float isoLevel)
        {
            int cubeIndex = 0;

            if (d0 > isoLevel) cubeIndex |= 1; // the |= operator sets every bit that is set in the right operand to 1 in the left operand also to 1.   
            // so if the left number (cubeIndex) is 0b11001000 and the right one (the mask, lets say 4) is 0b00000100 the result of that operation would be 0b11001100
            if (d1 > isoLevel) cubeIndex |= 2;
            if (d2 > isoLevel) cubeIndex |= 4;
            if (d3 > isoLevel) cubeIndex |= 8;
            if (d4 > isoLevel) cubeIndex |= 16;
            if (d5 > isoLevel) cubeIndex |= 32;
            if (d6 > isoLevel) cubeIndex |= 64;
            if (d7 > isoLevel) cubeIndex |= 128;

            return cubeIndex;
        }
    }
}
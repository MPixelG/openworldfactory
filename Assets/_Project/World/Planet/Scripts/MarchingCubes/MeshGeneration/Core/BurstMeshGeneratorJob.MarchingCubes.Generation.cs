using System.Runtime.CompilerServices;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public unsafe partial struct BurstMeshGeneratorJob
    {
        private void GenerateMarchingCubesCellAt(int3 pos)
        {
            int cubeIndex = GetMcCubeIndexAt(pos, Field, IsoLevel); // calculates the cube index at that position.
                                                                // look at the description of that function if you want to know what it does

            if (cubeIndex is 255 or 0) return;  // if the isosurface doesnt cut through any edge, we can skip this cube and return an empty list of triangles
            
            
            // helper vars for the single corners of the cube
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
            
            byte cellClass = RegularCellClass[cubeIndex]; //the cell class is one out of 16 cases of the cube.
                                                          //if we dont respect any rotation or mirroring, every cube variation is one out of those 16. 
            
            RegularCellData cellData = RegularCellData[cellClass]; //the cell data is the topology of that class case. it contains the triangle count and the edges it connects. 
            int triangleCount = cellData.TriangleCount;

            if (triangleCount == 0) return; //if it doesnt even contain any triangles, we can skip this cube

            
            // the resulting vertices can be 12 max since there are only 12 edges max. 
            Vtx* vertices = stackalloc Vtx[12];
            int vDataOffset = cubeIndex * 12;
            
            
            for(byte i = 0; i < triangleCount*3; i++) //since we know the amount of triangles we only need to repeat that times 3
                                                      //(i is the vertex index, every triangle has 3 vertices)
            {
                byte internalEdgeIndex = cellData.vertexIndex[i]; // we actually want to iterate over every edge that is used in
                                                          // the triangle configuration, so we take the edge index from the cell data.
                                                          // this way we dont need to check every edge and can just iterate over the used ones.
                if (internalEdgeIndex == 255) break;
                ushort vInfo = RegularVertexData[vDataOffset + internalEdgeIndex];
                byte edgeIndex = (byte)(vInfo & 0xFF);
                
                GetMcEdgeCorners(edgeIndex, out int cornerA, out int cornerB); // this returns the two corner indices this edge is in between. 
                
                int3 pA = corners[cornerA]; // here we get the corner positions
                int3 pB = corners[cornerB];
                
                vertices[edgeIndex].Pos =
                    VertexInterp(
                        IsoLevel, pA, pB,  // so we know now that this edge is used but if we just pass the center between the
                                           // start and end point of that edge it would be way too blocky. so we just interpolate between
                                           // the 2 densities of the corners of the edge. this way everything is distributed smoothly.
                        Field.DensityAt(pA), Field.DensityAt(pB)
                        );

                vertices[edgeIndex].Key = new VertexKey(pos, edgeIndex); // also store the key (consisting of the position and edge index)
                                                                         // so that we can deduplicate the vertices later
            }
            
            VoxelMaterial m0 = Field.MaterialAt(corners[0]); // here we get the materials at every corner
            VoxelMaterial m1 = Field.MaterialAt(corners[1]);
            VoxelMaterial m2 = Field.MaterialAt(corners[2]);
            VoxelMaterial m3 = Field.MaterialAt(corners[3]);
            VoxelMaterial m4 = Field.MaterialAt(corners[4]);
            VoxelMaterial m5 = Field.MaterialAt(corners[5]);
            VoxelMaterial m6 = Field.MaterialAt(corners[6]);
            VoxelMaterial m7 = Field.MaterialAt(corners[7]);

            //now we need to convert the vertices to triangles and take just the filled ones
            for (int i = 0; i < triangleCount * 3; i += 3)
            {
                byte internalA = cellData.vertexIndex[i];
                byte internalB = cellData.vertexIndex[i + 1];
                byte internalC = cellData.vertexIndex[i + 2];
                
                if (internalA == 255 || internalB == 255 || internalC == 255) break;

                byte a = (byte)(RegularVertexData[vDataOffset + internalA] & 0xFF);
                byte b = (byte)(RegularVertexData[vDataOffset + internalB] & 0xFF);
                byte c = (byte)(RegularVertexData[vDataOffset + internalC] & 0xFF);

                GetMcEdgeMaterials(a, m0, m1, m2, m3, m4, m5, m6, m7, out VoxelMaterial a0, out VoxelMaterial a1);
                GetMcEdgeMaterials(b, m0, m1, m2, m3, m4, m5, m6, m7, out VoxelMaterial b0, out VoxelMaterial b1); // and the 3 materials
                GetMcEdgeMaterials(c, m0, m1, m2, m3, m4, m5, m6, m7, out VoxelMaterial c0, out VoxelMaterial c1);

                VoxelMaterial material = GetDominantMaterial(a0, a1, b0, b1, c0, c1, MaterialCount); // now we get the dominant material

                AddTriangle(
                    vertices[a].Pos, vertices[a].Key,
                    vertices[b].Pos, vertices[b].Key, // and add the triangle consisting of the positions and keys of the indices and the material of the triangle
                    vertices[c].Pos, vertices[c].Key,
                    material
                );
            }
        } 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetMcEdgeCorners(int edge, out int cornerA, out int cornerB)
        {
            switch (edge)
            {
                case 0:  cornerA = 0; cornerB = 1; break;
                case 1:  cornerA = 1; cornerB = 3; break;
                case 2:  cornerA = 2; cornerB = 3; break;
                case 3:  cornerA = 0; cornerB = 2; break;

                case 4:  cornerA = 4; cornerB = 5; break;
                case 5:  cornerA = 5; cornerB = 7; break;
                case 6:  cornerA = 6; cornerB = 7; break;
                case 7:  cornerA = 4; cornerB = 6; break;

                case 8:  cornerA = 0; cornerB = 4; break;
                case 9:  cornerA = 1; cornerB = 5; break;
                case 10: cornerA = 2; cornerB = 6; break;
                case 11: cornerA = 3; cornerB = 7; break;

                default: cornerA = 0; cornerB = 0; break;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetMcEdgeMaterials(
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
                case 1:  a = m1; b = m3; break;
                case 2:  a = m2; b = m3; break;
                case 3:  a = m0; b = m2; break;

                case 4:  a = m4; b = m5; break;
                case 5:  a = m5; b = m7; break;
                case 6:  a = m6; b = m7; break;
                case 7:  a = m4; b = m6; break;

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
            
            if (grid.DensityAt(pos.x, pos.y, pos.z) > isoLevel) cubeIndex |= 1; // the |= operator sets every bit that is set in the right operand to 1 in the left operand also to 1.   
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
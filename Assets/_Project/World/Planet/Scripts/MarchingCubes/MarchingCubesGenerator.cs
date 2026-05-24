using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes
{
    public static class MarchingCubesGenerator
    {
        public static List<Triangle> GenerateAt(int3 pos, MarchingCubesGrid grid, float isoLevel)
        {
            int cubeIndex = GetCubeIndexAt(pos, grid, isoLevel); // calculates the cube index at that position. look at the description of that function if you want to know what it does
            
            int edgeFlags = McTables.EdgeTable[cubeIndex]; // converts the given corner configuration to a binary number where every bit represents one edge. note that we convert an 8 bit number (one bit for every corner) to a 12 bit number (one bit for every edge). 
            // we take that value from a huge precomputed table that contains the edge configuration for every possible corner configuration. So every bit is 1 if the isosurface cuts through that edge. 


            if (edgeFlags == 0) // if the isosurface doesnt cut through any edge, we can skip this cube and return an empty list of triangles
                return new List<Triangle>();

            Vector3[] vertices = new Vector3[12]; // the resulting vertices can be 12 max since there are only 12 edges. we fill the rest of the values with -1.
            
            // helper vars for the single corners of the cube
            int3 p0 = new int3(pos.x, pos.y, pos.z);
            int3 p1 = new int3(pos.x + 1, pos.y, pos.z);
            int3 p2 = new int3(pos.x + 1, pos.y+1, pos.z);
            int3 p3 = new int3(pos.x, pos.y+1, pos.z);
            int3 p4 = new int3(pos.x, pos.y, pos.z+1);
            int3 p5 = new int3(pos.x + 1, pos.y, pos.z+1);
            int3 p6 = new int3(pos.x + 1, pos.y + 1, pos.z + 1);
            int3 p7 = new int3(pos.x, pos.y + 1, pos.z + 1);
            
            
            // here we check if the given bit is 1. the binary operator & acts as an AND mask, so 0b11011101 & 0b01010011 = 0b01010001. this way we can check every edge separately. c# doesnt let you directly cast ints to bools (1=true, 0=false) so we just compare it to 0 to know if its true or not
            if ((edgeFlags & 1) != 0)
                vertices[0] = // so we know now that this edge is used but if we just pass the center between the start and end point of that edge it would be way to blocky. so we just interpolate between the 2 densities of the corresponding values. this way everything is distributed smoothly.
                    VertexInterp(isoLevel, p0, p1, grid.DensityAt(p0), grid.DensityAt(p1));
            if ((edgeFlags & 2) != 0)
                vertices[1] =
                    VertexInterp(isoLevel, p1, p2, grid.DensityAt(p1), grid.DensityAt(p2));
            if ((edgeFlags & 4) != 0)
                vertices[2] =
                    VertexInterp(isoLevel, p2, p3, grid.DensityAt(p2), grid.DensityAt(p3));
            if ((edgeFlags & 8) != 0)
                vertices[3] =
                    VertexInterp(isoLevel, p3, p0, grid.DensityAt(p3), grid.DensityAt(p0));
            if ((edgeFlags & 16) != 0)
                vertices[4] =
                    VertexInterp(isoLevel, p4, p5, grid.DensityAt(p4), grid.DensityAt(p5));
            if ((edgeFlags & 32) != 0)
                vertices[5] =
                    VertexInterp(isoLevel, p5, p6, grid.DensityAt(p5), grid.DensityAt(p6));
            if ((edgeFlags & 64) != 0)
                vertices[6] =
                    VertexInterp(isoLevel, p6, p7, grid.DensityAt(p6), grid.DensityAt(p7));
            if ((edgeFlags & 128) != 0)
                vertices[7] =
                    VertexInterp(isoLevel, p7, p4, grid.DensityAt(p7), grid.DensityAt(p4));
            if ((edgeFlags & 256) != 0)
                vertices[8] =
                    VertexInterp(isoLevel, p0, p4, grid.DensityAt(p0), grid.DensityAt(p4));
            if ((edgeFlags & 512) != 0)
                vertices[9] =
                    VertexInterp(isoLevel, p1, p5, grid.DensityAt(p1), grid.DensityAt(p5));
            if ((edgeFlags & 1024) != 0)
                vertices[10] =
                    VertexInterp(isoLevel, p2, p6, grid.DensityAt(p2), grid.DensityAt(p6));
            if ((edgeFlags & 2048) != 0)
                vertices[11] =
                    VertexInterp(isoLevel, p3, p7, grid.DensityAt(p3), grid.DensityAt(p7));
            
            
            //now we need to convert the vertices to triangles and take just the filled ones
            
            //so we have the used edges but we dont know how the triangles are supposed to be generated to occupy the given edge configuration. 
            // thats why we have another huge precomputed table that contains the triangle configuration for every possible edge configuration. so we just need to loop through that table until we find a -1 which marks the end of the triangle list for that edge configuration.
            
            List<Triangle> triangles = new List<Triangle>();
            for (int i=0;McTables.TriTable[cubeIndex, i]!=-1;i+=3) { // go in steps of 3 (a triangle consists of 3 points 🤯) until we find a -1 which marks the end of that triangle list
                Triangle triangle = new Triangle(
                    vertices[McTables.TriTable[cubeIndex, i]], //TODO vertex deduplication
                    vertices[McTables.TriTable[cubeIndex, i+1]],
                    vertices[McTables.TriTable[cubeIndex, i+2]]
                );
                triangles.Add(triangle);
            }

            return triangles;
        }


        // returns the cube index of a given position in a given grid using the given iso level as a threshold. the cube index is an 8 bit integer where every bit represents whether that corner is inside (1) or outside (0) of the isosurface. 
        private static int GetCubeIndexAt(int3 pos, MarchingCubesGrid grid, float isoLevel)
        {
            int cubeIndex = 0;

            if (grid.DensityAt(pos.x, pos.y, pos.z) < isoLevel) cubeIndex |= 1;// the |= operator sets every bit that is set in the right operand to 1 in the left operand also to 1.
                                                                               // so if the left number (cubeIndex) is 0b11001000 and the right one (the mask, lets say 4) is 0b00000100 the result of that operation would be 0b11001100
            if (grid.DensityAt(pos.x + 1, pos.y, pos.z) < isoLevel) cubeIndex |= 2;
            if (grid.DensityAt(pos.x + 1, pos.y+1, pos.z) < isoLevel) cubeIndex |= 4;
            if (grid.DensityAt(pos.x, pos.y+1, pos.z) < isoLevel) cubeIndex |= 8;
            if (grid.DensityAt(pos.x, pos.y, pos.z+1) < isoLevel) cubeIndex |= 16;
            if (grid.DensityAt(pos.x + 1, pos.y, pos.z+1) < isoLevel) cubeIndex |= 32;
            if (grid.DensityAt(pos.x + 1, pos.y + 1, pos.z + 1) < isoLevel) cubeIndex |= 64;
            if (grid.DensityAt(pos.x, pos.y + 1, pos.z + 1) < isoLevel) cubeIndex |= 128;

            return cubeIndex;
        }


        // lerps (linear interpolates) between 2 given points based on their density values and the iso level
        private static Vector3 VertexInterp(float isoLevel, int3 p1, int3 p2, float valP1, float valP2)
        {
            Vector3 p;

            if (Math.Abs(isoLevel - valP1) < 0.00001)
                return (new Vector3(p1.x, p1.y, p1.z));
            if (Math.Abs(isoLevel - valP2) < 0.00001)
                return (new Vector3(p2.x, p2.y, p2.z));
            if (Math.Abs(valP1 - valP2) < 0.00001)
                return (new Vector3(p1.x, p1.y, p1.z));
            float mu = (isoLevel - valP1) / (valP2 - valP1);
            p.x = p1.x + mu * (p2.x - p1.x);
            p.y = p1.y + mu * (p2.y - p1.y);
            p.z = p1.z + mu * (p2.z - p1.z);

            return (p);
        }
    }
}
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes
{
    
    public class MarchingCubesGenerator
    {
        private void GenerateAt(int3 pos, MarchingCubesGrid grid, float isoLevel)
        {

            int cubeIndex = GetCubeIndexAt(pos, grid, isoLevel);
            
            for (int i = 0; TriTable.TriTableVals[cubeIndex, i] != -1; i += 3)
            {
                int edgeA = TriTable.TriTableVals[cubeIndex, i];
                int edgeB = TriTable.TriTableVals[cubeIndex, i + 1];
                int edgeC = TriTable.TriTableVals[cubeIndex, i + 2];
                
                
                
                
            }
            
            

        }


        public static int GetCubeIndexAt(int3 pos, MarchingCubesGrid grid, float isoLevel)
        {
            int cubeIndex = 0;

            if (grid.DensityAt(pos.x, pos.y, pos.z) < isoLevel) cubeIndex |= 1;
            if (grid.DensityAt(pos.x+1, pos.y, pos.z) < isoLevel) cubeIndex |= 2;
            if (grid.DensityAt(pos.x+1, pos.y, pos.z+1) < isoLevel) cubeIndex |= 4;
            if (grid.DensityAt(pos.x, pos.y, pos.z+1) < isoLevel) cubeIndex |= 8;
            if (grid.DensityAt(pos.x, pos.y+1, pos.z) < isoLevel) cubeIndex |= 16;
            if (grid.DensityAt(pos.x+1, pos.y+1, pos.z) < isoLevel) cubeIndex |= 32;
            if (grid.DensityAt(pos.x+1, pos.y+1, pos.z+1) < isoLevel) cubeIndex |= 64;
            if (grid.DensityAt(pos.x, pos.y+1, pos.z+1) < isoLevel) cubeIndex |= 128;
            
            return cubeIndex;
        }
        
        
    }
}
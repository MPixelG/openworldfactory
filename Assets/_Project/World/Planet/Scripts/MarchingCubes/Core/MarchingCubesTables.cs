using System;
using Unity.Collections;

namespace _Project.World.Planet.Scripts.MarchingCubes.Core
{
    public class MarchingCubesTables : IDisposable
    {
        [ReadOnly]
        public NativeArray<int> EdgeTable;
        [ReadOnly]
        public NativeArray<int> TriTable;
        
        public void Dispose()
        {
            EdgeTable.Dispose();
            TriTable.Dispose();
        }

        public MarchingCubesTables()
        {
            EdgeTable = new NativeArray<int>(McTables.EdgeTable, Allocator.Persistent);

            int rows = McTables.TriTable.GetLength(0);
            int cols = McTables.TriTable.GetLength(1);

            TriTable = new NativeArray<int>(rows * cols, Allocator.Persistent);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    TriTable[i * cols + j] = McTables.TriTable[i, j];
                }
            }
        }
    }
}
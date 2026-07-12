using System.Runtime.InteropServices;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen.Parallel
{
    [BurstCompile]
    [StructLayout(LayoutKind.Auto)]
    public struct BurstSphericalNoiseSamplingJob : IJobParallelFor
    {
        public NativeArray<float> Densities;

        public byte Resolution;

        public NativeList<ulong> MortonCodes;
        
        public int3 Origin;
        public byte maxDepth;

        public BurstSphericalNoiseConfig Config;

        public void Execute(int mortonIndex)
        {
            ulong morton = MortonCodes[mortonIndex];
            int nodeSize = 1 << (maxDepth - morton.GetDepth());
            
            float3 minPos = Origin + morton.DecodeToCoord() * nodeSize;
            
            for (int index = 0; index < Resolution * Resolution * Resolution; index++)
            {

                float3 worldPos = ToWorldPos(minPos, nodeSize, index);
                float val = BurstSphericalNoiseGenerator.GenerateAt(worldPos, Config);
                
                Densities[index] = val;
            }
        }

        private float3 ToWorldPos(float3 min, int nodeSize, int index)
        {
            int x = index % Resolution;
            int y = (index / Resolution) % Resolution;
            int z = index / (Resolution * Resolution);
            
            if (Resolution == 1) return min + nodeSize*0.5f;

            float3 step = nodeSize / (Resolution - 1);
            return new float3(x, y, z) * step + min;
        }
    }
}
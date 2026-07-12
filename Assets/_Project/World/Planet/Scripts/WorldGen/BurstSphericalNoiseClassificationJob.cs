using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Parallel
{
    [BurstCompile]
    public struct BurstSphericalNoiseClassificationJob : IJobParallelFor
    {
        public NativeArray<MinMaxValue> Results;

        public byte Resolution;

        [ReadOnly] public NativeList<ulong> MortonCodes;

        public int3 Origin;
        public byte maxDepth;

        public BurstSphericalNoiseConfig Config;


        public void Execute(int mortonIndex)
        {
            float minVal = float.PositiveInfinity;
            float maxVal = float.NegativeInfinity;

            ulong morton = MortonCodes[mortonIndex];
            int nodeSize = 1 << (maxDepth - morton.GetDepth());

            float3 minPos = Origin + morton.DecodeToCoord() * nodeSize;
            
            for (int index = 0; index < Resolution * Resolution * Resolution; index++)
            {
                float3 worldPos = ToWorldPos(minPos, nodeSize, index);
                float val = BurstSphericalNoiseGenerator.GenerateAt(worldPos, Config);

                minVal = math.min(val, minVal);
                maxVal = math.max(val, maxVal);
            }
            Results[mortonIndex] = new MinMaxValue()
            {
                Min = minVal,
                Max = maxVal
            };
        }

        private float3 ToWorldPos(float3 min, int nodeSize, int index)
        {
            int x = index % Resolution;
            int y = (index / Resolution) % Resolution;
            int z = index / (Resolution * Resolution);

            if (Resolution <= 1) return min + nodeSize * 0.5f;

            float3 step = nodeSize / (Resolution - 1);
            return new float3(x, y, z) * step + min;
        }
    }

    public struct MinMaxValue
    {
        public float Min;
        public float Max;
    }
}
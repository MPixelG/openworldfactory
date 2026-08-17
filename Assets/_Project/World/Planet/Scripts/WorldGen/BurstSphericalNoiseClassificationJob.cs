using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen
{
    [BurstCompile]
    public struct BurstSphericalNoiseClassificationJob : IJobParallelFor
    {
        public NativeArray<MinMaxValue> Results;

        public byte Resolution;

        [ReadOnly] public NativeList<ulong> MortonCodes;

        public float3 Min;
        public float3 Max; 
        public byte MaxDepth;

        public BurstSphericalNoiseConfig Config;


        public void Execute(int mortonIndex)
        {
            float minVal = float.PositiveInfinity;
            float maxVal = float.NegativeInfinity;

            ulong morton = MortonCodes[mortonIndex];
            byte depth = morton.GetDepth(); 
            
            int logicalNodeSize = 1 << (MaxDepth - depth);
            int maxOctreeSize = 1 << MaxDepth;
            float3 nodeSize = (Max - Min)*((float)logicalNodeSize/maxOctreeSize);

            float3 localPos = morton.DecodeToCoord();
            float3 pos = Min + localPos * nodeSize;
            
            
            for (int index = 0; index < Resolution * Resolution * Resolution; index++)
            {
                float3 worldPos = ToWorldPos(pos, nodeSize, index);
                float val = BurstSphericalNoiseGenerator.GenerateAt(worldPos, Config).Item1;

                minVal = math.min(val, minVal);
                maxVal = math.max(val, maxVal);
            }
            Results[mortonIndex] = new MinMaxValue()
            {
                Min = minVal,
                Max = maxVal
            };
        }

        private float3 ToWorldPos(float3 min, float3 nodeSize, int index)
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
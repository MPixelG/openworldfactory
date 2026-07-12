using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen.Parallel
{
    [BurstCompile]
    public struct ParallelBurstSphericalNoiseSamplingJob : IJobParallelFor
    {
        public NativeArray<float> Densities;
        
        public byte Resolution;
        public int3 MinPos;
        public int3 MaxPos;

        public BurstSphericalNoiseConfig Config;

        public void Execute(int index)
        {
            float3 worldPos = ToWorldPos(index);
            float val = BurstSphericalNoiseGenerator.GenerateAt(worldPos, Config);
            Densities[index] = val;
        }

        private float3 ToWorldPos(int index)
        {
            int resolution = Resolution;
            int x = index % resolution;
            int y = (index / resolution) % resolution;
            int z = index / (resolution * resolution);
            
            if (resolution <= 1) return ((float3)MinPos + MaxPos) * 0.5f;

            float3 step = (MaxPos - MinPos) / (resolution - 1);
            return new float3(x, y, z) * step + MinPos;
        }
    }
}
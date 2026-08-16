using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.WorldGen.Parallel;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen
{
    [BurstCompile]
    public struct ParallelBurstSphericalNoiseSamplingJob : IJobParallelFor
    {
        public NativeArray<Voxel> Fields;
        
        public byte Resolution;
        public float3 Min;
        public float3 Max;

        public BurstSphericalNoiseConfig Config;

        public void Execute(int index)
        {
            Voxel voxel = new Voxel();
            float3 worldPos = ToWorldPos(index);
            
            float val = BurstSphericalNoiseGenerator.GenerateAt(worldPos, Config);
            voxel.Density = val;
            
            //use stone for high altitudes and dirt for low altitudes
            voxel.VoxelMaterial = worldPos.y > 200f ? VoxelMaterial.Stone : VoxelMaterial.Dirt;
            
            Fields[index] = voxel;
        }

        private float3 ToWorldPos(int index)
        {
            int x = index % Resolution;
            int y = (index / Resolution) % Resolution;
            int z = index / (Resolution * Resolution);

            if (Resolution <= 1) return Min + (Max-Min) * 0.5f;

            float3 step = (Max-Min) / (Resolution - 1f);
            return new float3(x, y, z) * step + Min;
        }
    }
}
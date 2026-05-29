using _Project.World.Planet.Scripts.WorldGen.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public static class DensityFieldBuilder
    {
        public static DensityField BuildBurstDensityField(BurstSamplerSettings settings, int size, int3 origin)
        {
            int gridSize = size + 1;
            
            NativeArray<float> densitiesOut = new NativeArray<float>(gridSize*gridSize*gridSize, Allocator.TempJob);
            
            BurstSphericalNoiseSamplerJob job = settings.CreateSampler(gridSize, origin);
            job.Densities = densitiesOut;
            
            
            JobHandle handle = job.Schedule(densitiesOut.Length, 64);
            handle.Complete();
            
            DensityField densityField = new(densitiesOut, gridSize);
            return densityField;
        }
    }
}
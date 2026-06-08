using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.WorldGen.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    /// <summary>
    /// a static builder class for building density fields using burst jobs
    /// </summary>
    public static class DensityFieldBuilder
    {
        /// <summary>
        /// generates a density field using sampler settings
        /// </summary>
        /// <param name="settings">the sampling settings used</param>
        /// <param name="size">the chunk size. Caution! the chunk size is the grid size - 1!</param>
        /// <param name="origin">the origin (start) position of the chunk. Caution! this is measured in grid chunk space!</param>
        /// <returns></returns>
        public static DensityField BuildBurstDensityField(BurstSamplerSettings settings, int size, ChunkCoord origin)
        {
            int gridSize = size + 1; // the grid size is the chunk size + 1 since we need to sample the density at the corners of the chunk as well for the marching cubes algorithm to work properly. todo use padding of 2 instead of 1
            
            // builds a native array (basically an array but its used for burst jobs since you cant use a lot of stuff there) with the required grid size.
            NativeArray<float> densitiesOut = new NativeArray<float>(gridSize*gridSize*gridSize, Allocator.TempJob);
            
            BurstSphericalNoiseSamplerJob job = settings.CreateSampler(gridSize, origin.Value * size); // get the job from the settings
            job.Densities = densitiesOut; // pass in the density array reference (it will get updated so we can read the results after the job is done)
            
            
            JobHandle handle = job.Schedule(densitiesOut.Length, 64); // schedule the job
            handle.Complete(); // and complete it
            
            DensityField densityField = new(densitiesOut, gridSize); // now we can pass the densities into the density field together with the grid size
            return densityField;
        }
    }
}
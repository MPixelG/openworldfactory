using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.WorldGen;
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
        /// <param name="size">the chunk size. Caution! the chunk size is the grid size - 2!</param>
        /// <param name="origin">the origin (start) position of the chunk. Caution! this is measured in grid chunk space!</param>
        /// <returns></returns>
        public static DensityFieldData BuildBurstDensityFieldData(BurstSamplerSettings settings, byte size, ChunkCoord origin)
        {
            byte gridSize = checked((byte)(size + 2)); // the grid size is the chunk size + 2 since we need to sample the density at the corners of the chunk as well for the marching cubes algorithm to work properly.
            
            // builds a native array (basically an array but its used for burst jobs since you cant use a lot of stuff there) with the required grid size.
            NativeArray<float> densitiesOut = new NativeArray<float>(gridSize*gridSize*gridSize, Allocator.Persistent);
            
            BurstSphericalNoiseSamplerJob job = settings.CreateSampler(origin.Value*size,origin.Value*size + gridSize, gridSize); // get the job from the settings
            job.Densities = densitiesOut; // pass in the density array reference (it will get updated so we can read the results after the job is done)
            
            
            JobHandle handle = job.Schedule(densitiesOut.Length, 64); // schedule the job
            handle.Complete(); // and complete it
            
            DensityFieldData densityField = new()
            {
                Densities = densitiesOut,
                Size = gridSize,
            }; // now we can pass the densities into the density field together with the grid size
            return densityField;
        }
        
        /// <summary>
        /// generates a density field using sampler settings (specialized for octree structures)
        /// </summary>
        /// <param name="settings">the sampling settings used</param>
        /// <param name="min">the start pos of that chunk</param>
        /// <param name="max">the end position of that chunk.</param>
        /// <param name="resolution">the grid size of the density field. Caution! this is the grid size, not the chunk size! the chunk size is calculated by resolution - 1 since we need to sample the density at the corners of the chunk as well for the marching cubes algorithm to work properly.</param>
        /// <returns></returns>
        public static DensityFieldData BuildBurstDensityFieldDataInTree(BurstSamplerSettings settings, int3 min, int3 max, byte resolution)
        {
            // builds a native array (basically an array but its used for burst jobs since you cant use a lot of stuff there) with the required grid size.
            NativeArray<float> densitiesOut = new NativeArray<float>(resolution*resolution*resolution, Allocator.Persistent);
            
            BurstSphericalNoiseSamplerJob job = settings.CreateSampler(min, max, resolution); // get the job from the settings
            job.Densities = densitiesOut; // pass in the density array reference (it will get updated so we can read the results after the job is done)
            
            
            JobHandle handle = job.Schedule(densitiesOut.Length, 64); // schedule the job
            handle.Complete(); // and complete it
            
            DensityFieldData densityField = new()
            {
                Densities = densitiesOut,
                Size = resolution,
            }; // now we can pass the densities into the density field together with the grid size
            return densityField;
        }
    }
}
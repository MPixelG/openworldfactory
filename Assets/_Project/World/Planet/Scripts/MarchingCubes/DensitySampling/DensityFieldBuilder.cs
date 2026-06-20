using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
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
        /// <param name="mortonCode">the morton code of the node in the tree</param>
        /// <param name="maxDepth">the maximum depth of the tree</param>
        /// <param name="origin">the origin position of the octree</param>
        /// <param name="resolution">the grid size of the density field. Caution! this is the grid size, not the chunk size! the chunk size is calculated by resolution - 1 since we need to sample the density at the corners of the chunk as well for the marching cubes algorithm to work properly.</param>
        /// <returns></returns>
        public static DensityFieldData BuildBurstDensityFieldDataInTree(BurstSamplerSettings settings, ulong mortonCode, byte maxDepth, int3 origin, byte resolution)
        {
            byte depth = mortonCode.GetDepth();
            int nodeSize = 1 << (maxDepth - depth);
            
            int3 localGridPos = mortonCode.DecodeToCoord();
            int3 min = origin + (localGridPos * nodeSize); 
            int3 max = min + new int3(nodeSize);
            
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
        
        /// <summary>
        /// schedules a density value generation job for a given area and resolution. it wont wait for the job to complete.
        /// you can check if the job is completed by calling <c> handle.isCompleted </c> and then calling <c> handle.Complete() </c> afterward.
        /// if you run <c> handle.Complete() </c> before the job is complete it will freeze the main thread (and thus the game) until the job is done.
        /// </summary>
        /// <param name="settings">the sampling settings used</param>
        /// <param name="mortonCode">the morton code of the node in the tree</param>
        /// <param name="maxDepth">the maximum depth of the tree</param>
        /// <param name="origin">the origin position of the octree</param>
        /// <param name="resolution">the grid size of the density field. Caution! this is the grid size, not the chunk size! the chunk size is calculated by resolution - 1 since we need to sample the density at the corners of the chunk as well for the marching cubes algorithm to work properly.</param>
        /// <returns></returns>
        public static JobHandle ScheduleBurstDensityFieldDataBuildInTree(BurstSamplerSettings settings, ulong mortonCode, byte maxDepth, int3 origin, byte resolution, out DensityFieldData densityField)
        {
            byte depth = mortonCode.GetDepth();
            int nodeSize = 1 << (maxDepth - depth);
            
            int3 localGridPos = mortonCode.DecodeToCoord();
            int3 min = origin + (localGridPos * nodeSize); 
            int3 max = min + new int3(nodeSize);
            
            NativeArray<float> densities = new NativeArray<float>(resolution*resolution*resolution, Allocator.Persistent); // builds a native array (basically an array but its used for burst jobs since you cant use a lot of stuff there) with the required grid size.
            
            BurstSphericalNoiseSamplerJob job = settings.CreateSampler(min, max, resolution); // get the job from the settings
            job.Densities = densities; // pass in the density array reference (it will get updated so we can read the results after the job is done)
            
            
            densityField = new DensityFieldData
            {
                Densities = densities,
                Size = resolution,
            }; // now we can pass the densities into the density field together with the grid size
            
            JobHandle handle = job.Schedule(densities.Length, 64); // schedule the job
            return handle;
        }
    }
}
using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.WorldGen;
using _Project.World.Planet.Scripts.WorldGen.Parallel;
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
        public static FieldData BuildBurstDensityFieldData(ParallelBurstSamplerSettings settings, byte size, ChunkCoord origin)
        {
            byte gridSize = checked((byte)(size+1)); // the grid size is the chunk size + 1 since we need to sample the density at the corners of the chunk as well for the marching cubes algorithm to work properly.
            
            // builds a native array (basically an array but its used for burst jobs since you cant use a lot of stuff there) with the required grid size.
            
            ParallelBurstSphericalNoiseSamplingJob job = settings.CreateParallelExactSampler(origin.Value*size,origin.Value*size + gridSize, gridSize, out FieldData densitiesOut); // get the job from the settings
            
            
            JobHandle handle = job.Schedule(densitiesOut.Fields.Length, 64); // schedule the job
            handle.Complete(); // and complete it
            
            return densitiesOut;
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
        public static JobHandle ScheduleExactBurstDensityFieldDataBuildInTree(ParallelBurstSamplerSettings settings, ulong mortonCode, float nodeSize, int3 origin, byte resolution, out FieldData field)
        {
            float3 localGridPos = mortonCode.DecodeToCoord();
            float3 min = origin + (localGridPos * (nodeSize)); 
            float3 max = min + nodeSize;
            
            ParallelBurstSphericalNoiseSamplingJob job = settings.CreateParallelExactSampler(min, max, resolution, out field); // get the job from the settings
            
            JobHandle handle = job.Schedule(field.Fields.Length, 64); // schedule the job
            return handle;
        }
    }
}
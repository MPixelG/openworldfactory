using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.MarchingCubes.BurstMeshGeneration;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen;
using _Project.World.Planet.Scripts.WorldGen.Burst;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking
{
    /// <summary>
    /// the chunk generator is responsible for generating the chunk data. this includes the density field and the mesh data. 
    /// </summary>
    public class ChunkGenerator
    {
        private readonly BurstSamplerSettings
            _densitySamplerSettings; // the settings used for generating the density field

        private readonly int _chunkSize;
        
        /// <summary>
        /// creates a chunk generator. the chunk generator is responsible for generating the chunk data (density values and mesh data).
        /// the density sampler settings are used for generating the density field and the chunk size is used for both
        /// the density field and the mesh data generation.
        /// </summary>
        /// <param name="densitySamplerSettings">the settings used for generation</param>
        /// <param name="chunkSize">the chunk size used for converting chunk to world space</param>
        public ChunkGenerator(BurstSamplerSettings densitySamplerSettings, int chunkSize)
        {
            _densitySamplerSettings = densitySamplerSettings;
            _chunkSize = chunkSize;
        }

        /// <summary>
        /// generates the chunk data at a given chunk position.
        /// </summary>
        /// <param name="position">the chunk position in chunk grid pos space</param>
        /// <returns>the chunk data of the chunk at the given position</returns>
        public ChunkData GenerateChunkAt(ChunkCoord position)
        {
            ChunkData data =
                new ChunkData // the initial unfilled chunk data with the coord and the state set to generating density
                {
                    Coord = position,
                    State = ChunkState.GeneratingDensity,
                };

            DensityFieldData densityField = DensityFieldBuilder.BuildBurstDensityFieldData(
                _densitySamplerSettings,
                _chunkSize,
                position
            ); // run a burst job to generate the density field for the chunk at the given position.
            // the world position is calculated by multiplying the chunk position with the chunk size.
            // this way we can use the same noise settings for all chunks and just
            // sample them at different positions in world space.

            data.DensityField = densityField; // update its density field with the newly calculated one
            data.State = ChunkState.Meshing; // and set its state to meshing

            // we use the static function GenerateMeshDataAt to generate the mesh data for the chunk based on its
            // density field and the chunk size
            //MeshData meshData = MarchingCubesMeshDataGenerator.GenerateMeshDataAt(densityField);
            MeshData meshData = BurstMeshGenerator.GenerateMesh(densityField);
            

            data.MeshData = meshData; // now we need to update the mesh data

            data.State = ChunkState.WaitingForMeshUpload; // and finally set the state to WaitingForMeshUpload since we
            // now wait for one of the next frames to upload the mesh to unity

            return data;
        }
    }
}
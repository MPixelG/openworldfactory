using System.Diagnostics;
using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.MarchingCubes.BurstMeshGeneration;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen.Burst;
using Debug = UnityEngine.Debug;

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
        
        private readonly BurstMeshGenerator _burstMeshGenerator;

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
            _burstMeshGenerator = new BurstMeshGenerator();
        }

        /// <summary>
        /// generates the chunk data at a given chunk position.
        /// </summary>
        /// <param name="position">the chunk position in chunk grid pos space</param>
        /// <returns>the chunk data of the chunk at the given position</returns>
        public ChunkData GenerateChunkAt(ChunkCoord position)
        {
            Stopwatch totalSw = Stopwatch.StartNew();

            ChunkData data = new ChunkData
            {
                Coord = position,
                State = ChunkState.GeneratingDensity,
            };

            // Density Generation messen
            Stopwatch densitySw = Stopwatch.StartNew();

            DensityFieldData densityField = DensityFieldBuilder.BuildBurstDensityFieldData(
                _densitySamplerSettings,
                _chunkSize,
                position
            );

            densitySw.Stop();

            data.DensityField = densityField;
            data.State = ChunkState.Meshing;

            // Mesh Generation messen
            Stopwatch meshSw = Stopwatch.StartNew();

            MeshData meshData = _burstMeshGenerator.GenerateBurstMesh(densityField);

            meshSw.Stop();

            data.MeshData = meshData;
            data.State = ChunkState.WaitingForMeshUpload;

            totalSw.Stop();

            Debug.Log(
                $"Chunk {position}: " +
                $"Density={densitySw.Elapsed.TotalMilliseconds:F2}ms, " +
                $"Mesh={meshSw.Elapsed.TotalMilliseconds:F2}ms, " +
                $"Total={totalSw.Elapsed.TotalMilliseconds:F2}ms"
            );

            return data;
        }
    }
}
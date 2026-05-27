using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking
{
    public class ChunkGenerator
    {
        private readonly IDensitySampler _densitySampler;
        private readonly int _chunkSize;
        
        public ChunkGenerator(IDensitySampler densitySampler, int chunkSize)
        {
            _densitySampler = densitySampler;
            _chunkSize = chunkSize;
        }

        public ChunkData GenerateChunkAt(ChunkCoord position)
        {
            ChunkData data = new ChunkData
            {
                Coord = position,
                State = ChunkState.GeneratingDensity,
                IsDirty = true,
            };
            
            DensityField densityField = DensityFieldBuilder.BuildDensityField(_densitySampler, _chunkSize, position.Value * _chunkSize);
            
            data.DensityField = densityField;
            data.State = ChunkState.Meshing;
            
            
            MeshData meshData = MarchingCubesMeshDataGenerator.GenerateMeshDataAt(_chunkSize, densityField);

            data.MeshData = meshData;
            
            data.State = ChunkState.WaitingForMeshUpload;

            return data;
        }
    }
}
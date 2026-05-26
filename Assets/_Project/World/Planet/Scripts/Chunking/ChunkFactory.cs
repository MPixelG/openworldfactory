using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.Chunking
{
    public class ChunkFactory
    {
        private readonly IDensitySampler _densitySampler;
        private readonly int _size;
        
        public ChunkFactory(IDensitySampler densitySampler, int size)
        {
            _densitySampler = densitySampler;
            _size = size;
        }

        public ChunkData GenerateChunkAt(int3 position)
        {
            ChunkData data = new ChunkData
            {
                Coord = new ChunkCoord(position.x, position.y, position.z),
                State = ChunkState.GeneratingDensity,
                IsDirty = true,
            };
            
            DensityField densityField = DensityFieldBuilder.BuildDensityField(_densitySampler, _size, position * _size);
            
            data.DensityField = densityField;
            data.State = ChunkState.Meshing;
            
            
            MeshData meshData = MarchingCubesMeshDataBuilder.GenerateMeshDataAt(_size, densityField);

            data.MeshData = meshData;
            
            data.State = ChunkState.WaitingForMeshUpload;

            return data;
        }
    }
}
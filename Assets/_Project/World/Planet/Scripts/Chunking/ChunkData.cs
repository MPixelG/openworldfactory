using _Project.World.Planet.Scripts.Chunking.Core;
using _Project.World.Planet.Scripts.MarchingCubes;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;

namespace _Project.World.Planet.Scripts.Chunking
{
    public class ChunkData
    {
        public ChunkCoord Coord;
        public MeshData MeshData;
        public DensityField DensityField;
        public ChunkState State;
        public bool IsDirty;
    }
}
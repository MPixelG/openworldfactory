using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using Unity.Collections;

namespace _Project.World.Planet.Scripts.Chunking.Core
{
    /// <summary>
    /// the chunk data contains all data of a chunk. besides its coord and state it also provides the current density field and mesh data.
    /// </summary>
    public class ChunkData
    {
        public ChunkCoord Coord;
        public MeshData MeshData;
        [ReadOnly]
        public DensityFieldData DensityField;
        public ChunkState State;
    }
}
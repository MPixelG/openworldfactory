using System.Collections.Generic;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core.Densities
{
    public class DensityStorage
    {
        private Dictionary<ulong, DensityFieldData> _densityFields;

        public DensityFieldData GetDensityFieldDataOf(ulong key)
        {
            return _densityFields[key];
        }

        public void SetDensityFieldDataOf(ulong key, DensityFieldData data)
        {
            _densityFields[key] = data;
        }

    }
}
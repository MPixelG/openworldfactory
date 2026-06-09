using Unity.Collections;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public struct DensityFieldData
    {
        public NativeArray<float> Densities;
        public int Size;

        public void Dispose()
        {
            if (Densities.IsCreated)
            {
                Densities.Dispose();
            }
        }
    }
}
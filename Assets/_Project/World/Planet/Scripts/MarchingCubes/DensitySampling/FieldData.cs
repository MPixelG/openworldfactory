using Unity.Collections;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public struct FieldData
    {
        public NativeArray<Voxel> Fields;
        public int Size;


        public void Dispose()
        {
            if (Fields.IsCreated)
            {
                Fields.Dispose();
            }
        }
    }
}
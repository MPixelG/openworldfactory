using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public class DensityField
    {
        private readonly NativeArray<float> _densities;
        private readonly int _size;
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // this tells the compiler to inline the function. that means it tries to put the content of this function into the place it was used. it is like you typed that formula out instead of calling the function there
        private static int IndexOf(int x, int y, int z, int size)
        {
            return x + y*size + z*size*size;
        }
        

        public float DensityAt(int3 position)
        {
            return DensityAt(position.x, position.y, position.z);
        }

        public float DensityAt(int x, int y, int z)
        {
            if (x < 0 || x >= _size || y < 0 || y >= _size || z < 0 || z >= _size)
            {
                return 0;
            }
            
            return _densities[IndexOf(x, y, z, _size)];
        }

        public DensityField(NativeArray<float> densities, int size)
        {
            _densities = densities;
            _size = size;
        }
    }
}
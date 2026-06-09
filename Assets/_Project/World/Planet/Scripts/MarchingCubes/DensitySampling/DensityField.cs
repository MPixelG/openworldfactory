using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public class DensityField
    {
        private NativeArray<float> _densities;
        public readonly int Size;
        
        /// <summary>
        /// creates a density field based of the given densities and a given size. the densities should be a 3d array stored inside a 1d array. the index should be calculated using <code>int index = x + y*size + z*size*size</code>
        /// </summary>
        /// <param name="densities">the density values used represented as a 1d array</param>
        /// <param name="size">the grid size. caution! the grid size should be 2 higher than the chunk size!</param>
        public DensityField(NativeArray<float> densities, int size)
        {
            _densities = densities;
            Size = size;
        }
        
        /// <summary>
        /// calculates a unique 1d index for storing the 3d grid in a 1d array. we use a 1d array since it is more performant to store and access and there will be a lot of reads on this table.
        /// </summary>
        /// <returns>a 1d index based of the 3d position and grid square size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // this tells the compiler to inline the function. that means it tries to put the content of this function into the place it was used. it is like you typed that formula out instead of calling the function there
        private static int IndexOf(int x, int y, int z, int size)
        {
            return x + y*size + z*size*size;
        }
        
        /// <summary>
        /// returns the density value at a given position. caution! this does not interpolate between positions so it only returns values for values that are exactly on the requested grid positions.
        /// </summary>
        /// <param name="position">the grid position of the requested density value</param>
        /// <returns>the density of the given position. it is usually a value between 0 and 1.</returns>
        public float DensityAt(int3 position)
        {
            return DensityAt(position.x, position.y, position.z);
        }

        /// <summary>
        /// returns the density value at a given position. caution! this does not interpolate between positions so it only returns values for values that are exactly on the requested grid positions.
        /// </summary>
        /// <param name="x">the x position of the requested density value</param>
        /// <param name="y">the y position of the requested density value</param>
        /// <param name="z">the z position of the requested density value</param>
        /// <returns>the density of the given position. it is usually a value between 0 and 1.</returns>
        public float DensityAt(int x, int y, int z)
        {
            x++;
            y++;
            z++;
            
            if (x < 0) x = 0;
            if (x >= Size) x = Size - 1;
            if (y < 0) y = 0;
            if (y >= Size) y = Size - 1;
            if (z < 0) z = 0;
            if (z >= Size) z = Size - 1;
            
            return _densities[IndexOf(x, y, z, Size)]; // return the density value at the given position. we calculate the index in the 1d array based on the x, y and z values and the size of the grid.
        }

        /// <summary>
        /// returns an interpolated density value at a given position. this is used to get smoother results when the position is not exactly on the grid. it uses trilinear interpolation to calculate the density value based on the 8 surrounding grid points.
        /// </summary>
        /// <param name="position">the position of the requested density</param>
        /// <returns>the interpolated density value at that position</returns>
        public float DensityAt(float3 position)
        {
            int3 posInt = (int3)math.floor(position); // get the integer part of the position. this is the position of the grid point that is closest to the given position but still smaller than it.
            float3 posFrac = position - posInt; // get the fractional part of the position. this is the distance from the closest grid point to the given position. it is a value between 0 and 1.
            
            // get the density values of the 8 surrounding grid points1
            float d000 = DensityAt(posInt.x, posInt.y, posInt.z);
            float d001 = DensityAt(posInt.x, posInt.y, posInt.z + 1);
            float d010 = DensityAt(posInt.x, posInt.y + 1, posInt.z);
            float d011 = DensityAt(posInt.x, posInt.y + 1, posInt.z + 1);
            float d100 = DensityAt(posInt.x + 1, posInt.y, posInt.z);
            float d101 = DensityAt(posInt.x + 1, posInt.y, posInt.z + 1);
            float d110 = DensityAt(posInt.x + 1, posInt.y + 1, posInt.z);
            float d111 = DensityAt(posInt.x + 1, posInt.y + 1, posInt.z + 1);

            // perform trilinear interpolation
            float d00 = math.lerp(d000, d100, posFrac.x); // interpolate between d000 and d100 based on the x fractional part
            float d01 = math.lerp(d001, d101, posFrac.x); // interpolate between d001 and d101 based on the x fractional part
            float d10 = math.lerp(d010, d110, posFrac.x); // interpolate between d010 and d110 based on the x fractional part
            float d11 = math.lerp(d011, d111, posFrac.x); // interpolate between d011 and d111 based on the x fractional part
            
            float d0 = math.lerp(d00, d10, posFrac.y); // interpolate between d00 and d10 based on the y fractional part
            float d1 = math.lerp(d01, d11, posFrac.y); // interpolate between d01 and d11 based on the y fractional part
            
            return math.lerp(d0, d1, posFrac.z); // interpolate between d0 and d1 based on the z fractional part and return the result
        }

        public void Dispose()
        {
            _densities.Dispose();
        }
    }
}
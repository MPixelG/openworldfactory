using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public static class DensityBurstHelper
    {
        /// <summary>
        /// returns the density value at a given position. caution! this does not interpolate between positions so it only returns values for values that are exactly on the requested grid positions.
        /// </summary>
        /// <param name="data">the density field data used</param>
        /// <param name="position">the grid position of the requested density value</param>
        /// <returns>the density of the given position. it is usually a value between 0 and 1.</returns>
        public static float DensityAt(this DensityFieldData data, int3 position)
        {
            return DensityAt(data, position.x, position.y, position.z);
        }
        

        /// <summary>
        /// returns the density value at a given position. caution! this does not interpolate between positions so it only returns values for values that are exactly on the requested grid positions.
        /// </summary>
        /// <param name="x">the x position of the requested density value</param>
        /// <param name="y">the y position of the requested density value</param>
        /// <param name="z">the z position of the requested density value</param>
        /// <returns>the density of the given position. it is usually a value between 0 and 1.</returns>
        public static float DensityAt(this DensityFieldData data, int x, int y, int z)
        {
            x++;
            y++;
            z++;
            
            if (x < 0) x = 0;
            if (x >= data.Size) x = data.Size - 1;
            if (y < 0) y = 0;
            if (y >= data.Size) y = data.Size - 1;
            if (z < 0) z = 0;
            if (z >= data.Size) z = data.Size - 1;
            
            return data.Densities[IndexOf(x, y, z, data.Size)]; // return the density value at the given position. we calculate the index in the 1d array based on the x, y and z values and the size of the grid.
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
    }
}
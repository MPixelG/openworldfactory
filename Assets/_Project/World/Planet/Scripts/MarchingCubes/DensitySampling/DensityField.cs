using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.DensitySampling
{
    public class DensityField
    {
        private readonly NativeArray<float> _densities;
        public readonly int Size;
        
        /// <summary>
        /// creates a density field based of the given densities and a given size. the densities should be a 3d array stored inside a 1d array. the index should be calculated using <code>int index = x + y*size + z*size*size</code>
        /// </summary>
        /// <param name="densities">the density values used represented as a 1d array</param>
        /// <param name="size">the grid size. caution! the grid size should be 1 higher than the chunk size!</param> // todo padding of 2 instead of 1
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
            if (x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size)
            {
                return 0; // if the requested position is outside the bounds we just return 0.
            }
            
            return _densities[IndexOf(x, y, z, Size)]; // return the density value at the given position. we calculate the index in the 1d array based on the x, y and z values and the size of the grid.
        }
    }
}
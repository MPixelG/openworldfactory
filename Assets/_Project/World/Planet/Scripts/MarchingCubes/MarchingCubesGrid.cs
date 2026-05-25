using System;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes
{
    
    /// <summary>
    /// this class contains the density values for a marching cubes chunk.
    /// you can think of it as a 3D array of density values, where each value represents the density at a specific point in space.
    /// the marching cubes algorithm will use these density values to determine where to create vertices and triangles for the mesh.
    /// </summary>
    public class MarchingCubesGrid
    {
        private float[,,] _densities; // 3d array of density values
        private readonly int _size; // the size of the density array

        private readonly TerrainGenerator _generator;

        public MarchingCubesGrid(int size, TerrainGenerator terrainGenerator)
        {
            _generator = terrainGenerator;
            _size = size;
            BuildDensities();
        }

        /// <summary>
        /// this builds the densities. its only called once.
        /// </summary>
        private void BuildDensities()
        {
            _densities = new float[_size + 1, _size + 1, _size + 1];
            
            for (int x = 0; x < _size + 1; x++)
            {
                for (int y = 0; y < _size + 1; y++)
                {
                    for (int z = 0; z < _size + 1; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z);

                        _densities[x, y, z] = SampleDensity(
                            pos
                        );
                    }
                }
            }
            
            
        }

        /// <summary>
        /// this is the density sampling function. it gets called for every point in the grid. so you could pass a distance function to the center of the grid to get a sphere. take a look at sdfs (signed distance functions).
        /// </summary>
        private float SampleDensity(
            Vector3 pos
        )
        {
            return _generator.DensityAt(pos);
        }


        public float DensityAt(int3 pos)
        {
            if(pos.x < 0 || pos.x > _size || pos.y < 0 || pos.y > _size || pos.z < 0 || pos.z > _size)
                return 0;
            return _densities[pos.x, pos.y, pos.z];
        }
        
        public float DensityAt(int x, int y, int z)
        {
            if(x < 0 || x > _size || y < 0 || y > _size || z < 0 || z > _size)
                return 0;
            return _densities[x, y, z];
        }

        public void ForEach(Action<int3, float> action)
        {
            for(int x = 0; x < _size+1; x++)
            {
                for (int y = 0; y < _size+1; y++)
                {
                    for (int z = 0; z < _size+1; z++)
                    {
                        action(new int3(x, y, z), DensityAt(x, y, z));
                    }
                }
            }
            
        }
    }
}
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
        private readonly Vector3 _worldMin;
        private readonly Vector3 _worldMax;
        private readonly Vector3 _pos; // the origin of the grid in world space

        private readonly TerrainGenerator _generator;

        public MarchingCubesGrid(int size, TerrainGenerator terrainGenerator, Vector3 pos, Vector3 worldMin, Vector3 worldMax)
        {
            _generator = terrainGenerator;
            _size = size;
            _worldMin = worldMin;
            _worldMax = worldMax;
            _pos = pos;
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
                        _densities[x, y, z] = _generator.DensityAt(new Vector3Int(x, y, z) + _pos);
                    }
                }
            }
        }

        public float DensityAt(int3 pos)
        {
            return DensityAt(pos.x, pos.y, pos.z);
        }
        
        public float DensityAt(int x, int y, int z)
        {
            if(x < 0 || x > _size || y < 0 || y > _size || z < 0 || z > _size)
                return 0;

            Vector3 worldPos = new Vector3(x, y, z) + _pos;

            if (worldPos.x <= _worldMin.x ||
                worldPos.y <= _worldMin.y || // if the position is at the border we also want to return that its empty space to prevent clipping. you can temporarily delete this block to see its effect. 
                worldPos.z <= _worldMin.z ||
                worldPos.x >= _worldMax.x ||
                worldPos.y >= _worldMax.y ||
                worldPos.z >= _worldMax.z)
            {
                return 0f;
            }
            
            
            //Debug.Log("comparing world pos " + worldPos + " to world min " + _worldMin + " and world max " + _worldMax);
            

            return _densities[x, y, z];
        }

        public void ForEach(Action<int3, float> action)
        {
            for(int x = 0; x < _size; x++)
            {
                for (int y = 0; y < _size; y++)
                {
                    for (int z = 0; z < _size; z++)
                    {
                        action(new int3(x, y, z), DensityAt(x, y, z));
                    }
                }
            }
            
        }
    }
}
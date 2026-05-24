using System;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.MarchingCubes
{
    public class MarchingCubesGrid
    {
        private float[,,] _densities;
        private readonly int _size;

        public MarchingCubesGrid(int size)
        {
            _size = size;
            BuildDensities();
        }

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

                        _densities[x, y, z] = SampleDensity(pos, new Vector3(_size/2f, _size/2f, _size/2f), _size/2f);
                    }
                }
            }
            
            
        }

        private static float SampleDensity(Vector3 pos, Vector3 center, float radius)
        {
            //for now just a simple sphere sdf, later we can add noise and other features to make it more interesting
            return radius - Vector3.Distance(pos, center);
        }


        public float DensityAt(int3 pos)
        {
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
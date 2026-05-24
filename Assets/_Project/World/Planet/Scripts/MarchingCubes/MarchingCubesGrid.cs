using System;
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
        private readonly float _noiseFrequency; // the frequency of the noise. it scales the input to the noise function so lower frequency = less noise, higher frequency = more noise
        private readonly float _noiseAmplitude; // the amount of noise that gets added
        private readonly float _noiseBias; // a minimum noise value that gets added so that the sphere always has a fraction of its size, even if the noise is negative

        public MarchingCubesGrid(int size, float noiseFrequency, float noiseAmplitude, float noiseBias)
        {
            _size = size;
            _noiseFrequency = noiseFrequency;
            _noiseAmplitude = noiseAmplitude;
            _noiseBias = noiseBias;
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
                            pos,
                            new Vector3(_size / 2f, _size / 2f, _size / 2f),
                            _size / 2f,
                            _noiseFrequency,
                            _noiseAmplitude,
                            _noiseBias
                        );
                    }
                }
            }
            
            
        }

        /// <summary>
        /// this is the density sampling function. it gets called for every point in the grid. so you could pass a distance function to the center of the grid to get a sphere. take a look at sdfs (signed distance functions).
        /// </summary>
        public static float SampleDensity(
            Vector3 pos,
            Vector3 sphereCenter,
            float radius,
            float noiseFrequency,
            float noiseAmplitude,
            float noiseBias
        )
        {
            float sphereSdf = radius - Vector3.Distance(pos, sphereCenter);

            float rawNoise = noise.cnoise(pos * noiseFrequency);

            float noiseValue = rawNoise * noiseAmplitude;
            
            return sphereSdf * (noiseValue + noiseBias);
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
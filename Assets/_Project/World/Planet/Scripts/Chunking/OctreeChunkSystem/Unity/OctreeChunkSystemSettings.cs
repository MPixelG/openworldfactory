using System;
using _Project.World.Planet.Scripts.WorldGen;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity
{
    public class OctreeChunkSystemSettings : MonoBehaviour
    {
        [SerializeField] public Vector3Int origin; // the size of a chunk
        [SerializeField] public float size;
        
        [SerializeField] private BurstSamplerSettings densitySamplerSettings; // this contains all the noise and world gen settings  

        public OctreeChunkManager CreateManager()
        {
            if (densitySamplerSettings == null) // the density sampler settings are required for creating the chunk manager so we have to throw an error if it is not specified
            {
                throw new InvalidOperationException("Density sampler settings are not assigned.");
            }

            return new OctreeChunkManager(origin, size, densitySamplerSettings);
        }
    }
}
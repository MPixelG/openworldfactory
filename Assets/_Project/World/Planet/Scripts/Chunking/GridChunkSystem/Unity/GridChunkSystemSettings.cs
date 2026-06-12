using System;
using _Project.World.Planet.Scripts.WorldGen;
using _Project.World.Planet.Scripts.WorldGen.Burst;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem.Unity
{
    /// <summary>
    /// the settings contain everything thats customizable about the system. so visual settings, world generation settings and everything else we will add later 
    /// </summary>
    [CreateAssetMenu(menuName = "WorldGen/Chunking/Grid Chunk System")]
    public class GridChunkSystemSettings : ScriptableObject
    {
        [Min(1)] [SerializeField] private byte chunkSize = 16; // the size of a chunk
        [Min(0)] [SerializeField] private int viewDistanceInChunks = 4; // this view distance is the radius of the sphere of chunks to keep around the viewer
        [SerializeField] private BurstSamplerSettings densitySamplerSettings; // this contains all the noise and world gen settings  

        public GridChunkManager CreateManager()
        {
            if (densitySamplerSettings == null) // the density sampler settings are required for creating the chunk manager so we have to throw an error if it is not specified
            {
                throw new InvalidOperationException("Density sampler settings are not assigned.");
            }

            return new GridChunkManager(chunkSize, viewDistanceInChunks, densitySamplerSettings);
        }
    }
}

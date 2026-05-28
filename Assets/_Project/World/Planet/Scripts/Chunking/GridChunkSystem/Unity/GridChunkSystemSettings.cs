using System;
using _Project.World.Planet.Scripts.WorldGen.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem.Unity
{
    [CreateAssetMenu(menuName = "WorldGen/Chunking/Grid Chunk System")]
    public class GridChunkSystemSettings : ScriptableObject
    {
        [Min(1)] [SerializeField] private int chunkSize = 16;
        [Min(0)] [SerializeField] private int viewDistanceInChunks = 4;
        [SerializeField] private DensitySamplerSettings densitySampler;

        public GridChunkManager CreateManager()
        {
            if (densitySampler == null)
            {
                throw new InvalidOperationException("Density sampler settings are not assigned.");
            }

            return new GridChunkManager(chunkSize, viewDistanceInChunks, densitySampler.CreateSampler());
        }
    }
}

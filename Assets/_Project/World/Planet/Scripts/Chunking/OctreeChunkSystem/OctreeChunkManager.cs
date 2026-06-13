using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem
{
    public class OctreeChunkManager
    {
        public Octree Octree;

        public int3 Origin;
        public float Size;
        
        private BurstSamplerSettings _densitySamplerSettings;
        

        /// <param name="origin">the origin world pos of the system</param>
        /// <param name="size">the size of the system in world space</param>
        /// <param name="densitySamplerSettings">the settings used for generating the density values</param>
        public OctreeChunkManager(
            Vector3Int origin,
            float size,
            BurstSamplerSettings densitySamplerSettings
        )
        {
            Origin = new int3(origin.x, origin.y, origin.z);
            Size = size;

            _densitySamplerSettings = densitySamplerSettings;

            RebuildOctree();
        }

        public void RebuildOctree()
        {
            Octree = OctreeHelper.Build(Origin, new int3((int) Size, (int) Size, (int) Size), _densitySamplerSettings, 5);
        }

        public void Update(float3 viewerPos)
        {
            
        }
    }
}
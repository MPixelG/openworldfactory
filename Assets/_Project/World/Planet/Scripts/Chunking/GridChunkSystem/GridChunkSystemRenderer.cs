using System;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.WorldGen;
using _Project.World.Planet.Scripts.WorldGen.Samplers;
using UnityEngine;

namespace _Project.World.Planet.Scripts.Chunking.GridChunkSystem
{
    [ExecuteAlways]
    public class GridChunkSystemRenderer : MonoBehaviour
    {
        private GridChunkManager _chunkManager;


        public void SetChunkManager(GridChunkManager chunkManager)
        {
            _chunkManager = chunkManager;
        }
        
        private void FixedUpdate()
        {
            _chunkManager?.Update();
        }
    }
}
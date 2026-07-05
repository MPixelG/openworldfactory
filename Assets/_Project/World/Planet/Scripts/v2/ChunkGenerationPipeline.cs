using System;
using System.Collections.Generic;
using System.Linq;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration;
using _Project.World.Planet.Scripts.v2.Data;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.v2
{
    public class ChunkGenerationPipeline
    {
        private readonly Queue<ulong> _generationQueue = new();
        private readonly Dictionary<ulong, JobHandle> _activeMeshingJobs = new();
        private readonly Dictionary<ulong, ChunkPayload> _activeMeshingJobResults = new();
        private readonly Dictionary<ulong, NativeHashMap<VertexKey, int>> _meshingJobGarbage = new();

        private BurstSamplerSettings _settings;
        private byte _maxDepth;
        private int3 _min;
        private byte _chunkSize;

        public event Action<ChunkGeneration> OnChunkGenerated;


        public void UpdateSamplerSettings(BurstSamplerSettings settings)
        {
            _settings = settings;
        }

        public void UpdateMaxDepth(byte maxDepth)
        {
            _maxDepth = maxDepth;
        }

        public void UpdateMin(int3 min)
        {
            _min = min;
        }

        public void UpdateChunkSize(byte chunkSize)
        {
            _chunkSize = chunkSize;
        }

        public void Update()
        {
            ProcessNextInQueue();
            UpdateFinishedJobs();
        }

        public void QueueGenerationAt(ulong mortonCode)
        {
            _generationQueue.Enqueue(mortonCode);
        }

        private void ProcessNextInQueue()
        {
            if (_generationQueue.Count == 0) return;

            ulong mortonCode = _generationQueue.Dequeue();



            JobHandle densityJobHandle = DensityFieldBuilder.ScheduleBurstDensityFieldDataBuildInTree(
                _settings,
                mortonCode,
                _maxDepth,
                _min,
                (byte)(_chunkSize+1),
                out DensityFieldData densityField
            );

            JobHandle meshGenerationJobHandle = BurstMeshGenerator.ScheduleGenerateMesh(densityJobHandle, densityField,
                out NativeList<int> indices, out NativeList<float3> vertices, out NativeList<float3> normals, out NativeHashMap<VertexKey, int> vertexMap);

            ChunkPayload payload = new ChunkPayload
            {
                DensityField = densityField,
                Vertices = vertices,
                Normals = normals,
                Indices = indices
            };

            _activeMeshingJobs.Add(mortonCode, meshGenerationJobHandle);
            _activeMeshingJobResults.Add(mortonCode, payload);
            _meshingJobGarbage.Add(mortonCode, vertexMap);
        }

        private void UpdateFinishedJobs()
        {
            for (var i = 0; i < _activeMeshingJobs.Keys.Count; i++)
            {
                ulong mortonCode = _activeMeshingJobs.Keys.ElementAt(i);

                if (!_activeMeshingJobs.TryGetValue(mortonCode, out JobHandle jobHandle)) continue;

                bool completed = jobHandle.IsCompleted;
                if (!completed) continue;

                jobHandle.Complete();

                OnChunkGenerated?.Invoke(new ChunkGeneration
                {
                    Payload = _activeMeshingJobResults[mortonCode], 
                    MortonCode = mortonCode
                });
                
                _meshingJobGarbage[mortonCode].Dispose();

                _activeMeshingJobs.Remove(mortonCode);
                _activeMeshingJobResults.Remove(mortonCode);
                _meshingJobGarbage.Remove(mortonCode);
                i--;
            }
        }
    }

    public struct ChunkGeneration
    {
        public ChunkPayload Payload;
        public ulong MortonCode;
    }
}
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen.Burst
{
    [BurstCompile]
    public struct BurstSphericalNoiseSamplerJob : IJobParallelFor
    {
        public NativeArray<float> Densities;
        public int Size;
        public int3 Origin;
        public float Radius;
        
        public void Execute(int index)
        {
            int x = index % Size;
            int y = (index / Size) % Size;
            int z = index / (Size * Size);
            
            float3 worldPos = new float3(x, y, z) + Origin;
            float density = math.distance(worldPos, float3.zero) - Radius;

            float3 dirToCenter = math.normalizesafe(-worldPos);
            
            float noiseValRaw = MountainNoise(worldPos * 0.01f + dirToCenter * 0.5f);
            float noiseVal = noiseValRaw * (Radius/2);
            Densities[index] = density + noiseVal;
        }
        
        private static float NoiseOctaved(float3 pos, int octaves, float persistence)
        {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                total += MountainNoise(pos) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }
        
        private static float MountainNoise(float3 pos)
        {
            float3 warped = pos;

            float3 warp = new float3(
                noise.cnoise(pos * 0.5f),
                noise.cnoise(pos * 0.5f + 17),
                noise.cnoise(pos * 0.5f + 33)
            ) * 0.4f;

            warped += warp;

            float n = noise.cnoise(warped * 2f);

            // ridged mountains
            n = 1f - math.abs(n);
            n = math.pow(n, 5f);

            return n;
        }
        
    }
}
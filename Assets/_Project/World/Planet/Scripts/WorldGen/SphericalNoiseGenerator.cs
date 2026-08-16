using _Project.World.Planet.Scripts.WorldGen.Parallel;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    public struct BurstSphericalNoiseGenerator
    {
        
        
        public static float GenerateAt(float3 worldPos, BurstSphericalNoiseConfig config)
        {
            float dist = math.length(worldPos);

            float sphereDensity = dist - config.Radius;
            
            float3 warpTmp = new float3(
                noise.cnoise(worldPos*0.1f * config.WarpFrequency + 17f),
                noise.cnoise(worldPos*0.1f * config.WarpFrequency + 53f),
                noise.cnoise(worldPos*0.1f * config.WarpFrequency + 91f)
            );
            
            float res_ = RidgedNoise(
                warpTmp * config.MountainMaskFrequency*250f,
                2,
                0.5f
            );
            return res_;
            
            float3 dir = math.normalizesafe(worldPos);
            

            float scale = config.Radius;

            float3 samplePos = dir * scale;
            

            float3 warp = new float3(
                noise.cnoise(samplePos * config.WarpFrequency + 17f),
                noise.cnoise(samplePos * config.WarpFrequency + 53f),
                noise.cnoise(samplePos * config.WarpFrequency + 91f)
            );

            float3 warpedPos = samplePos + warp * config.WarpStrength;
            

            float mountainMask = RidgedNoise(
                warpedPos * config.MountainMaskFrequency,
                4,
                0.5f
            );

            mountainMask = mountainMask * 0.5f + 0.5f;

            mountainMask = math.smoothstep(
                config.MountainThreshold,
                config.MountainThreshold + config.MountainBlend,
                mountainMask
            );
            

            float mountains = FractalNoise(
                warpedPos * config.MountainFrequency,
                config.MountainOctaves,
                config.MountainPersistence, 2, 0.7f
            );
            
            mountains = mountains * 0.5f + 0.5f;

            mountains = math.pow(
                mountains,
                config.MountainSharpness
            );

            mountains *= mountainMask;
            

            float plains = FractalNoise(
                warpedPos * config.PlainsFrequency,
                2,
                0.5f, 1, 1
            );

            plains *= config.PlainsStrength;

            plains *= math.min(1f - mountainMask, 1);
            

            float detail = noise.cnoise(
                warpedPos * config.DetailFrequency
            );

            detail *= config.DetailStrength;

            detail *= math.lerp(
                0.3f,
                1f,
                mountainMask
            );


            float terrain = mountains * config.TerrainHeight * config.Radius + plains + detail;

            return (sphereDensity - terrain);
        }
        

        private static float FractalNoise(
            float3 pos,
            int octaves,
            float persistence,
            float startSharpness,
            float sharpnessPersistence
            )
        {
            float total = 0f;
            float amplitude = 1f;
            float sharpness = startSharpness;
            float frequency = 1f;
            float maxValue = 0f;

            for (int i = 0; i < octaves; i++)
            {
                total += noise.cnoise(pos * frequency) * amplitude;

                maxValue += math.pow(amplitude, sharpness);
                
                amplitude *= persistence;
                frequency *= 2f;
                sharpness *= sharpnessPersistence;
            }

            return total / maxValue;
        }

        private static float RidgedNoise(
            float3 pos,
            int octaves,
            float persistence)
        {
            float total = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float maxValue = 0f;

            for (int i = 0; i < octaves; i++)
            {
                float n = noise.cnoise(pos * frequency);

                n = 1f - math.abs(n);

                n *= n;

                total += n * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2f;
            }

            return total / maxValue;
        }
    }
}
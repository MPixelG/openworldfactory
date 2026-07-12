using Unity.Jobs;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen
{
    public partial struct BurstSphericalNoiseSamplerJob
    {
        
        
        public float GenerateAt(float3 worldPos)
        {
            float dist = math.length(worldPos);

            float sphereDensity = dist - Radius;
            
            float3 dir = math.normalizesafe(worldPos);
            

            float scale = ReferenceRadius / Radius;

            float3 samplePos = dir * scale;
            

            float continent = FractalNoise(
                samplePos * ContinentFrequency,
                ContinentOctaves,
                ContinentPersistence
            );

            continent = continent * 0.5f + 0.5f;

            continent = math.smoothstep(
                OceanThreshold,
                1f,
                continent
            );
            

            float3 warp = new float3(
                noise.cnoise(samplePos * WarpFrequency + 17f),
                noise.cnoise(samplePos * WarpFrequency + 53f),
                noise.cnoise(samplePos * WarpFrequency + 91f)
            );

            float3 warpedPos = samplePos + warp * WarpStrength;
            

            float mountainMask = FractalNoise(
                warpedPos * MountainMaskFrequency,
                3,
                0.5f
            );

            mountainMask = mountainMask * 0.5f + 0.5f;

            mountainMask = math.smoothstep(
                MountainThreshold,
                MountainThreshold + MountainBlend,
                mountainMask
            );

            mountainMask *= continent;
            

            float mountains = RidgedNoise(
                warpedPos * MountainFrequency,
                MountainOctaves,
                MountainPersistence
            );

            mountains = math.pow(
                mountains,
                MountainSharpness
            );

            mountains *= mountainMask;
            

            float plains = FractalNoise(
                warpedPos * PlainsFrequency,
                2,
                0.5f
            );

            plains *= PlainsStrength;

            plains *= (1f - mountainMask);
            

            float detail = noise.cnoise(
                warpedPos * DetailFrequency
            );

            detail *= DetailStrength;

            detail *= math.lerp(
                0.3f,
                1f,
                mountainMask
            );
            

            float terrain =
                mountains * TerrainHeight +
                plains +
                detail;

            terrain -= TerrainHeight * 0.15f * continent;

            return (sphereDensity + terrain) + math.clamp(FractalNoise(worldPos/25f, 3, 0.3f), 0, 1)*Radius;
        }
        

        private static float FractalNoise(
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
                total += noise.cnoise(pos * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2f;
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
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen.Parallel
{
    public struct BurstSphericalNoiseGenerator
    {
        
        
        public static float GenerateAt(float3 worldPos, BurstSphericalNoiseConfig config)
        {
            float dist = math.length(worldPos);

            float sphereDensity = dist - config.Radius;
            
            float3 dir = math.normalizesafe(worldPos);
            

            float scale = config.ReferenceRadius / config.Radius;

            float3 samplePos = dir * scale;
            

            float continent = FractalNoise(
                samplePos * config.ContinentFrequency,
                config.ContinentOctaves,
                config.ContinentPersistence
            );

            continent = continent * 0.5f + 0.5f;

            continent = math.smoothstep(
                config.OceanThreshold,
                1f,
                continent
            );
            

            float3 warp = new float3(
                noise.cnoise(samplePos * config.WarpFrequency + 17f),
                noise.cnoise(samplePos * config.WarpFrequency + 53f),
                noise.cnoise(samplePos * config.WarpFrequency + 91f)
            );

            float3 warpedPos = samplePos + warp * config.WarpStrength;
            

            float mountainMask = FractalNoise(
                warpedPos * config.MountainMaskFrequency,
                3,
                0.5f
            );

            mountainMask = mountainMask * 0.5f + 0.5f;

            mountainMask = math.smoothstep(
                config.MountainThreshold,
                config.MountainThreshold + config.MountainBlend,
                mountainMask
            );

            mountainMask *= continent;
            

            float mountains = RidgedNoise(
                warpedPos * config.MountainFrequency,
                config.MountainOctaves,
                config.MountainPersistence
            );

            mountains = math.pow(
                mountains,
                config.MountainSharpness
            );

            mountains *= mountainMask;
            

            float plains = FractalNoise(
                warpedPos * config.PlainsFrequency,
                2,
                0.5f
            );

            plains *= config.PlainsStrength;

            plains *= (1f - mountainMask);
            

            float detail = noise.cnoise(
                warpedPos * config.DetailFrequency
            );

            detail *= config.DetailStrength;

            detail *= math.lerp(
                0.3f,
                1f,
                mountainMask
            );
            

            float terrain =
                mountains * config.TerrainHeight +
                plains +
                detail;

            terrain -= config.TerrainHeight * 0.15f * continent;

            return (sphereDensity + terrain) + math.clamp(FractalNoise(worldPos/25f, 3, 0.3f), 0, 1)*config.Radius;
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
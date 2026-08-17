using _Project.World.Planet.Scripts.WorldGen;

namespace _Project.Dev.WorldGenDebug.Scripts
{
    /// <summary>
    /// Contains preset configurations for world generation noise parameters.
    /// Use these to quickly test different world types and styles.
    /// </summary>
    public static class WorldGenPresets
    {
        /// <summary>
        /// Default balanced configuration.
        /// </summary>
        public static BurstSphericalNoiseConfig GetDefaultConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.17f,
                ContinentOctaves = 7,
                ContinentPersistence = 0.5f,
                ContinentFrequency = 0.005f,
                MountainMaskFrequency = 0.02f,
                MountainThreshold = 0.72f,
                MountainBlend = 0.3f,
                MountainFrequency = 0.002f,
                MountainOctaves = 12,
                MountainPersistence = 0.45f,
                MountainSharpness = 1f,
                PlainsStrength = 20f,
                PlainsFrequency = 0.01f,
                DetailFrequency = 0.3f,
                DetailStrength = 0.008f,
                WarpFrequency = 0.01f,
                WarpStrength = 1f,
            };
        }

        /// <summary>
        /// Beach world - sandy, flat terrain with minimal elevation.
        /// </summary>
        public static BurstSphericalNoiseConfig GetBeachWorldConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.1f,
                ContinentOctaves = 5,
                ContinentPersistence = 0.4f,
                ContinentFrequency = 0.008f,
                MountainMaskFrequency = 0.03f,
                MountainThreshold = 0.85f,
                MountainBlend = 0.2f,
                MountainFrequency = 0.003f,
                MountainOctaves = 8,
                MountainPersistence = 0.4f,
                MountainSharpness = 0.5f,
                PlainsStrength = 15f,
                PlainsFrequency = 0.015f,
                DetailFrequency = 0.25f,
                DetailStrength = 0.005f,
                WarpFrequency = 0.008f,
                WarpStrength = 0.5f,
            };
        }

        /// <summary>
        /// Mountainous world - rocky peaks and high elevation changes.
        /// </summary>
        public static BurstSphericalNoiseConfig GetMountainousWorldConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.3f,
                ContinentOctaves = 8,
                ContinentPersistence = 0.6f,
                ContinentFrequency = 0.003f,
                MountainMaskFrequency = 0.015f,
                MountainThreshold = 0.5f,
                MountainBlend = 0.4f,
                MountainFrequency = 0.0015f,
                MountainOctaves = 16,
                MountainPersistence = 0.5f,
                MountainSharpness = 2.5f,
                PlainsStrength = 5f,
                PlainsFrequency = 0.005f,
                DetailFrequency = 0.4f,
                DetailStrength = 0.015f,
                WarpFrequency = 0.012f,
                WarpStrength = 2f,
            };
        }

        /// <summary>
        /// Archipelago - island world with many small land masses separated by water.
        /// </summary>
        public static BurstSphericalNoiseConfig GetArchipelagoConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.2f,
                ContinentOctaves = 9,
                ContinentPersistence = 0.3f,
                ContinentFrequency = 0.02f,
                MountainMaskFrequency = 0.025f,
                MountainThreshold = 0.6f,
                MountainBlend = 0.25f,
                MountainFrequency = 0.002f,
                MountainOctaves = 10,
                MountainPersistence = 0.45f,
                MountainSharpness = 1.5f,
                PlainsStrength = 10f,
                PlainsFrequency = 0.012f,
                DetailFrequency = 0.35f,
                DetailStrength = 0.01f,
                WarpFrequency = 0.015f,
                WarpStrength = 1.5f,
            };
        }

        /// <summary>
        /// Earth-like world - realistic terrain with balanced features.
        /// </summary>
        public static BurstSphericalNoiseConfig GetEarthLikeConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.175f,
                ContinentOctaves = 8,
                ContinentPersistence = 0.55f,
                ContinentFrequency = 0.004f,
                MountainMaskFrequency = 0.018f,
                MountainThreshold = 0.68f,
                MountainBlend = 0.35f,
                MountainFrequency = 0.0018f,
                MountainOctaves = 13,
                MountainPersistence = 0.48f,
                MountainSharpness = 1.2f,
                PlainsStrength = 18f,
                PlainsFrequency = 0.011f,
                DetailFrequency = 0.32f,
                DetailStrength = 0.009f,
                WarpFrequency = 0.01f,
                WarpStrength = 1.2f,
            };
        }

        /// <summary>
        /// Flat world - minimal elevation changes, peaceful terrain.
        /// </summary>
        public static BurstSphericalNoiseConfig GetFlatWorldConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.08f,
                ContinentOctaves = 4,
                ContinentPersistence = 0.3f,
                ContinentFrequency = 0.01f,
                MountainMaskFrequency = 0.04f,
                MountainThreshold = 0.9f,
                MountainBlend = 0.1f,
                MountainFrequency = 0.004f,
                MountainOctaves = 5,
                MountainPersistence = 0.3f,
                MountainSharpness = 0.3f,
                PlainsStrength = 25f,
                PlainsFrequency = 0.02f,
                DetailFrequency = 0.2f,
                DetailStrength = 0.003f,
                WarpFrequency = 0.005f,
                WarpStrength = 0.3f,
            };
        }

        /// <summary>
        /// Dense jungle - lots of variation and detail, organic feeling.
        /// </summary>
        public static BurstSphericalNoiseConfig GetJungleWorldConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.22f,
                ContinentOctaves = 10,
                ContinentPersistence = 0.65f,
                ContinentFrequency = 0.006f,
                MountainMaskFrequency = 0.035f,
                MountainThreshold = 0.4f,
                MountainBlend = 0.5f,
                MountainFrequency = 0.0025f,
                MountainOctaves = 14,
                MountainPersistence = 0.52f,
                MountainSharpness = 1.8f,
                PlainsStrength = 25f,
                PlainsFrequency = 0.025f,
                DetailFrequency = 0.5f,
                DetailStrength = 0.02f,
                WarpFrequency = 0.02f,
                WarpStrength = 2.5f,
            };
        }

        /// <summary>
        /// Cavernous world - extreme elevation variation with deep caverns and tall peaks.
        /// </summary>
        public static BurstSphericalNoiseConfig GetCavernousWorldConfig(float radius = 100f)
        {
            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = 0.4f,
                ContinentOctaves = 6,
                ContinentPersistence = 0.7f,
                ContinentFrequency = 0.002f,
                MountainMaskFrequency = 0.01f,
                MountainThreshold = 0.3f,
                MountainBlend = 0.6f,
                MountainFrequency = 0.001f,
                MountainOctaves = 18,
                MountainPersistence = 0.6f,
                MountainSharpness = 3f,
                PlainsStrength = 2f,
                PlainsFrequency = 0.002f,
                DetailFrequency = 0.6f,
                DetailStrength = 0.03f,
                WarpFrequency = 0.025f,
                WarpStrength = 3f,
            };
        }
    }
}


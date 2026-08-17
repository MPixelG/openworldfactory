using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Unity;
using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Dev.WorldGenDebug.Scripts
{
    /// <summary>
    /// Real-time debug visualizer for world generation.
    /// Displays a 2D height map preview that updates when parameters change.
    /// All settings can be tweaked in the editor and the preview updates in real-time.
    /// </summary>
    [ExecuteAlways]
    public class WorldGenDebugVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private MaterialDatabase materialDatabase;

        [Header("Visualization Settings")]
        [SerializeField, Range(4, 2048)]
        private int heightMapWidth = 512;

        [SerializeField, Range(4, 2048)]
        private int heightMapHeight = 256;

        [SerializeField]
        private bool showMaterialDifferences = true;

        [SerializeField]
        private bool autoUpdate = true;

        [Header("Noise Configuration")]
        [SerializeField, Range(1f, 1000f)]
        private float radius = 100f;

        [SerializeField, Range(0f, 1f)]
        private float terrainHeight = 0.17f;

        [Header("Continent Settings")]
        [SerializeField, Range(1, 20)]
        private int continentOctaves = 7;

        [SerializeField, Range(0f, 1f)]
        private float continentPersistence = 0.5f;

        [SerializeField, Range(0f, 0.15f)]
        private float continentFrequency = 0.005f;

        [Header("Mountain Settings")]
        [SerializeField, Range(0f, 0.1f)]
        private float mountainMaskFrequency = 0.02f;

        [SerializeField, Range(0f, 1f)]
        private float mountainThreshold = 0.72f;

        [SerializeField, Range(0f, 1f)]
        private float mountainBlend = 0.3f;

        [SerializeField, Range(0f, 0.1f)]
        private float mountainFrequency = 0.002f;

        [SerializeField, Range(1, 20)]
        private int mountainOctaves = 12;

        [SerializeField, Range(0f, 1f)]
        private float mountainPersistence = 0.45f;

        [SerializeField, Range(0.1f, 12f)]
        private float mountainSharpness = 1f;

        [Header("Plains Settings")]
        [SerializeField, Range(0f, 100f)]
        private float plainsStrength = 20f;

        [SerializeField, Range(0f, 0.1f)]
        private float plainsFrequency = 0.01f;

        [Header("Detail Settings")]
        [SerializeField, Range(0f, 1f)]
        private float detailFrequency = 0.3f;

        [SerializeField, Range(0f, 0.1f)]
        private float detailStrength = 0.008f;

        [Header("Warp Settings")]
        [SerializeField, Range(0f, 0.1f)]
        private float warpFrequency = 0.01f;

        [SerializeField, Range(0f, 2f)]
        private float warpStrength = 1f;

        private Texture2D _heightMapTexture;
        private Material _displayMaterial;
        private bool _parametersChanged = true;

        private void OnEnable()
        {
            if (_displayMaterial == null)
            {
                _displayMaterial = new Material(Shader.Find("Unlit/Texture"));
            }

            _parametersChanged = true;
        }

        private void OnDisable()
        {
            if (_heightMapTexture != null)
            {
                DestroyImmediate(_heightMapTexture);
                _heightMapTexture = null;
            }
        }

        private void Update()
        {
            if (autoUpdate && (_parametersChanged || _heightMapTexture == null))
            {
                RegenerateHeightMap();
                _parametersChanged = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (autoUpdate)
            {
                _parametersChanged = true;
            }
        }
#endif

        /// <summary>
        /// Manually regenerates the height map. Called automatically if autoUpdate is enabled.
        /// </summary>
        public void RegenerateHeightMap()
        {
            var config = BuildNoiseConfig();

            // Clean up old texture
            if (_heightMapTexture != null)
            {
                DestroyImmediate(_heightMapTexture);
            }

            // Generate new height map
            _heightMapTexture = HeightMapGenerator2D.GenerateHeightMapTexture(
                heightMapWidth,
                heightMapHeight,
                config,
                showMaterialDifferences
            );

            _heightMapTexture.name = "HeightMapDebug";

            // Update material for display
            if (_displayMaterial != null)
            {
                _displayMaterial.mainTexture = _heightMapTexture;
            }
        }

        /// <summary>
        /// Gets the current height map texture.
        /// </summary>
        public Texture2D GetHeightMapTexture()
        {
            return _heightMapTexture;
        }

        public BurstSphericalNoiseConfig GetConfig()
        {
            return BuildNoiseConfig();
        }

        public float GetBaseRadius()
        {
            return radius;
        }

        public MaterialDatabase GetMaterialDatabase()
        {
            return materialDatabase;
        }

        /// <summary>
        /// Gets the display material for the height map.
        /// </summary>
        public Material GetDisplayMaterial()
        {
            return _displayMaterial;
        }

        /// <summary>
        /// Samples the world generation at a specific latitude and longitude.
        /// </summary>
        public (float height, VoxelMaterial material) SampleAtLatitudeLongitude(
            float latitude,
            float longitude)
        {
            return HeightMapGenerator2D.GenerateHeightAt(
                latitude,
                longitude,
                BuildNoiseConfig()
            );
        }

        /// <summary>
        /// Samples the world generation at a 3D world position.
        /// </summary>
        public (float density, VoxelMaterial material) SampleAt3D(float3 worldPos)
        {
            return NoiseGenerator3D.GenerateAt(worldPos, BuildNoiseConfig());
        }

        /// <summary>
        /// Gets the material color for a given voxel material type.
        /// </summary>
        public Color GetMaterialColor(VoxelMaterial material)
        {
            return material switch
            {
                VoxelMaterial.Air => new Color(135f / 255f, 206f / 255f, 235f / 255f),    // Sky blue
                VoxelMaterial.Dirt => new Color(139f / 255f, 69f / 255f, 19f / 255f),     // Brown
                VoxelMaterial.Stone => new Color(128f / 255f, 128f / 255f, 128f / 255f),  // Gray
                VoxelMaterial.Water => new Color(30f / 255f, 144f / 255f, 255f / 255f),   // Dodger blue
                VoxelMaterial.Sand => new Color(238f / 255f, 214f / 255f, 175f / 255f),   // Wheat
                VoxelMaterial.Grass => new Color(34f / 255f, 139f / 255f, 34f / 255f),    // Forest green
                VoxelMaterial.Snow => new Color(255f / 255f, 250f / 255f, 250f / 255f),   // Snow white
                VoxelMaterial.Lava => new Color(255f / 255f, 69f / 255f, 0f / 255f),      // Red-orange
                _ => Color.white
            };
        }

        private BurstSphericalNoiseConfig BuildNoiseConfig()
        {
            heightMapWidth = math.max(8, heightMapWidth);
            heightMapHeight = math.max(4, heightMapHeight);

            return new BurstSphericalNoiseConfig
            {
                Radius = radius,
                TerrainHeight = terrainHeight,
                ContinentFrequency = continentFrequency,
                ContinentOctaves = continentOctaves,
                ContinentPersistence = continentPersistence,
                MountainMaskFrequency = mountainMaskFrequency,
                MountainThreshold = mountainThreshold,
                MountainBlend = mountainBlend,
                MountainFrequency = mountainFrequency,
                MountainOctaves = mountainOctaves,
                MountainPersistence = mountainPersistence,
                MountainSharpness = mountainSharpness,
                PlainsStrength = plainsStrength,
                PlainsFrequency = plainsFrequency,
                DetailFrequency = detailFrequency,
                DetailStrength = detailStrength,
                WarpFrequency = warpFrequency,
                WarpStrength = warpStrength,
            };
        }
    }
}



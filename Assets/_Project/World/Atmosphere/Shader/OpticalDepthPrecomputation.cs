using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mathf;

namespace _Project.World.Atmosphere.Shader
{
    [CreateAssetMenu(menuName = "Celestial Body/Atmosphere")]
    public class AtmosphereSettings : ScriptableObject
    {
        public bool enabled = true;

        public ComputeShader opticalDepthCompute;

        [Min(1)]
        public int textureSize = 256;

        public float densityFalloff = 0.25f;

        public Vector3 wavelengths = new(700, 530, 460);

        public float scatteringStrength = 20;
        public float intensity = 1;

        public float ditherStrength = 0.8f;
        public float ditherScale = 4;
        public Texture2D blueNoise;

        [Range(0, 1)]
        public float atmosphereScale = 0.5f;

        private RenderTexture _opticalDepthTexture;
        private RTHandle _opticalDepthHandle;
        private bool _opticalDepthDirty = true;

        private static readonly int BakedOpticalDepth =
            UnityEngine.Shader.PropertyToID("_BakedOpticalDepth");

        private static readonly int DirToSun = UnityEngine.Shader.PropertyToID("dir_to_sun");
        private static readonly int BlueNoise = UnityEngine.Shader.PropertyToID("_BlueNoise");
        private static readonly int DitherScale = UnityEngine.Shader.PropertyToID("dither_scale");
        private static readonly int DitherStrength = UnityEngine.Shader.PropertyToID("dither_strength");
        private static readonly int Intensity = UnityEngine.Shader.PropertyToID("intensity");
        private static readonly int ScatteringCoefficients = UnityEngine.Shader.PropertyToID("scattering_coefficients");
        private static readonly int DensityFalloff = UnityEngine.Shader.PropertyToID("density_falloff");
        private static readonly int AtmosphereRadius = UnityEngine.Shader.PropertyToID("atmosphere_radius");
        private static readonly int OceanRadius = UnityEngine.Shader.PropertyToID("ocean_radius");
        private static readonly int PlanetRadius = UnityEngine.Shader.PropertyToID("planet_radius");
        private static readonly int PlanetCenter = UnityEngine.Shader.PropertyToID("planet_center");

        
        public void SetProperties(
            Material material,
            Vector3 planetCentre,
            float bodyRadius)
        {
            if (material == null)
                return;

            float atmosphereRadius =
                (1f + atmosphereScale) * bodyRadius;
            
            material.SetVector(
                PlanetCenter,
                planetCentre
            );

            material.SetFloat(
                PlanetRadius,
                bodyRadius
            );

            material.SetFloat(
                OceanRadius,
                bodyRadius
            );

            material.SetFloat(
                AtmosphereRadius,
                atmosphereRadius
            );

            material.SetFloat(
                DensityFalloff,
                densityFalloff
            );

            float scatterX = Pow(400f / wavelengths.x, 4f);
            float scatterY = Pow(400f / wavelengths.y, 4f);
            float scatterZ = Pow(400f / wavelengths.z, 4f);

            material.SetVector(
                ScatteringCoefficients,
                new Vector3(scatterX, scatterY, scatterZ)
                * scatteringStrength
            );

            material.SetFloat(Intensity, intensity);
            material.SetFloat(DitherStrength, ditherStrength);
            material.SetFloat(DitherScale, ditherScale);
            material.SetTexture(BlueNoise, blueNoise);

            
            var sun = GameObject.Find("Test Sun");
            if (sun == null) return;

            Vector3 sunPosition =
                sun.transform.position;

            material.SetVector(
                DirToSun,
                (sunPosition - planetCentre).normalized
            );
        }


        public bool NeedsOpticalDepthPrecompute =>
            _opticalDepthDirty ||
            _opticalDepthTexture == null ||
            !_opticalDepthTexture.IsCreated();

        
        private RenderTexture GetOrCreateOpticalDepthTexture()
        {
            if (_opticalDepthTexture != null &&
                _opticalDepthTexture.IsCreated() &&
                _opticalDepthTexture.width == textureSize &&
                _opticalDepthTexture.height == textureSize)
            {
                return _opticalDepthTexture;
            }

            ReleaseOpticalDepthTexture();

            _opticalDepthTexture = new RenderTexture(
                textureSize,
                textureSize,
                0,
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear
            )
            {
                name = $"{name}_BakedOpticalDepth",
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            _opticalDepthTexture.Create();

            MarkOpticalDepthDirty();

            return _opticalDepthTexture;
        }
        
        public RTHandle GetOpticalDepthHandle()
        {
            if (_opticalDepthHandle == null)
            {
                RenderTexture texture =
                    GetOrCreateOpticalDepthTexture();

                _opticalDepthHandle =
                    RTHandles.Alloc(texture);
            }

            return _opticalDepthHandle;
        }

        
        public void MarkOpticalDepthDirty()
        {
            _opticalDepthDirty = true;
        }


        public void MarkOpticalDepthClean()
        {
            _opticalDepthDirty = false;
        }


        public void SetOpticalDepthTexture(Material material)
        {
            if (material == null)
                return;

            material.SetTexture(
                BakedOpticalDepth,
                _opticalDepthTexture
            );
        }


        private void OnValidate()
        {
            textureSize = Max(1, textureSize);
            MarkOpticalDepthDirty();
        }


        private void OnDisable()
        {
            ReleaseOpticalDepthTexture();
        }


        private void OnDestroy()
        {
            ReleaseOpticalDepthTexture();
        }


        private void ReleaseOpticalDepthTexture()
        {
            if (_opticalDepthHandle != null)
            {
                _opticalDepthHandle.Release();
                _opticalDepthHandle = null;
            }

            if (_opticalDepthTexture == null) return;
            
            if (_opticalDepthTexture.IsCreated())
                _opticalDepthTexture.Release();

            DestroyImmediate(_opticalDepthTexture);
            _opticalDepthTexture = null;
        }
    }
}
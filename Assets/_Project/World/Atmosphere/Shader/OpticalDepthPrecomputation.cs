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
        public int opticalDepthTextureSize = 256;

        public float atmosphereDensityFalloff = 0.25f;

        public Vector3 atmosphereWavelengths = new(700, 530, 460);

        public float atmosphereScatteringStrength = 20;
        public float atmosphereIntensity = 1;

        public float atmosphereDitherStrength = 0.8f;
        public float atmosphereDitherScale = 4;
        public Texture2D atmosphereBlueNoise;

        [Range(0, 1)]
        public float atmosphereScale = 0.5f;
        

        public RenderTexture _opticalDepthTexture;
        public RTHandle _opticalDepthHandle;
        public bool _opticalDepthDirty = true;

        private static readonly int BakedOpticalDepth =
            UnityEngine.Shader.PropertyToID("_BakedOpticalDepth");

        private static readonly int AtmosphereDirToSun = UnityEngine.Shader.PropertyToID("dir_to_sun");
        private static readonly int AtmosphereBlueNoise = UnityEngine.Shader.PropertyToID("_BlueNoise");
        private static readonly int AtmosphereDitherScale = UnityEngine.Shader.PropertyToID("dither_scale");
        private static readonly int AtmosphereDitherStrength = UnityEngine.Shader.PropertyToID("dither_strength");
        private static readonly int AtmosphereIntensity = UnityEngine.Shader.PropertyToID("intensity");
        private static readonly int AtmosphereScatteringCoefficients = UnityEngine.Shader.PropertyToID("scattering_coefficients");
        private static readonly int AtmosphereDensityFalloff = UnityEngine.Shader.PropertyToID("density_falloff");
        private static readonly int AtmosphereRadius = UnityEngine.Shader.PropertyToID("atmosphere_radius");
        private static readonly int AtmospherePlanetRadius = UnityEngine.Shader.PropertyToID("planet_radius");
        private static readonly int AtmospherePlanetCenter = UnityEngine.Shader.PropertyToID("planet_center");
        
        
        
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
                AtmospherePlanetCenter,
                planetCentre
            );

            material.SetFloat(
                AtmospherePlanetRadius,
                bodyRadius
            );

            material.SetFloat(
                AtmosphereRadius,
                atmosphereRadius
            );

            material.SetFloat(
                AtmosphereDensityFalloff,
                atmosphereDensityFalloff
            );

            float scatterX = Pow(400f / atmosphereWavelengths.x, 4f);
            float scatterY = Pow(400f / atmosphereWavelengths.y, 4f);
            float scatterZ = Pow(400f / atmosphereWavelengths.z, 4f);

            material.SetVector(
                AtmosphereScatteringCoefficients,
                new Vector3(scatterX, scatterY, scatterZ)
                * atmosphereScatteringStrength
            );

            material.SetFloat(AtmosphereIntensity, atmosphereIntensity);
            material.SetFloat(AtmosphereDitherStrength, atmosphereDitherStrength);
            material.SetFloat(AtmosphereDitherScale, atmosphereDitherScale);
            material.SetTexture(AtmosphereBlueNoise, atmosphereBlueNoise);

            
            var sun = GameObject.Find("Test Sun");
            if (sun == null) return;

            Vector3 sunPosition =
                sun.transform.position;

            material.SetVector(
                AtmosphereDirToSun,
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
                _opticalDepthTexture.width == opticalDepthTextureSize &&
                _opticalDepthTexture.height == opticalDepthTextureSize)
            {
                return _opticalDepthTexture;
            }

            ReleaseOpticalDepthTexture();

            _opticalDepthTexture = new RenderTexture(
                opticalDepthTextureSize,
                opticalDepthTextureSize,
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
            // Re-allocate handle if null or if texture was resized
            if (_opticalDepthHandle != null && _opticalDepthHandle.rt.width == opticalDepthTextureSize &&
                _opticalDepthHandle.rt.height == opticalDepthTextureSize) return _opticalDepthHandle;
            
            
            _opticalDepthHandle?.Release();

            // Ensure explicit fixed size allocation
            _opticalDepthHandle = RTHandles.Alloc(GetOrCreateOpticalDepthTexture());

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
            opticalDepthTextureSize = Max(1, opticalDepthTextureSize);
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
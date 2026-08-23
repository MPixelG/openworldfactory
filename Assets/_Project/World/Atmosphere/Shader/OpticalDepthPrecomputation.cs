using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mathf;

namespace _Project.World.Atmosphere.Shader
{
    [CreateAssetMenu(menuName = "Celestial Body/Atmosphere")]
    public class AtmosphereSettings : ScriptableObject
    {
        public bool enabled = true;

        public UnityEngine.Shader atmosphereShader;
        public ComputeShader opticalDepthCompute;

        [Min(1)]
        public int textureSize = 256;

        [Min(1)]
        public int inScatteringPoints = 10;

        [Min(1)]
        public int opticalDepthPoints = 10;

        public float densityFalloff = 0.25f;

        public Vector3 wavelengths = new(700, 530, 460);

        public float scatteringStrength = 20;
        public float intensity = 1;

        public float ditherStrength = 0.8f;
        public float ditherScale = 4;
        public Texture2D blueNoise;

        [Range(0, 1)]
        public float atmosphereScale = 0.5f;

        [Header("Test")]
        public float timeOfDay;
        public float sunDst = 1;

        private RenderTexture opticalDepthTexture;
        private RTHandle opticalDepthHandle;
        private bool opticalDepthDirty = true;

        private static readonly int AtmosphereRadius =
            UnityEngine.Shader.PropertyToID("atmosphereRadius");

        private static readonly int PlanetRadius =
            UnityEngine.Shader.PropertyToID("planetRadius");

        private static readonly int DensityFalloff =
            UnityEngine.Shader.PropertyToID("densityFalloff");

        private static readonly int ScatteringCoefficients =
            UnityEngine.Shader.PropertyToID("scatteringCoefficients");

        private static readonly int Intensity =
            UnityEngine.Shader.PropertyToID("intensity");

        private static readonly int DitherStrength =
            UnityEngine.Shader.PropertyToID("ditherStrength");

        private static readonly int DitherScale =
            UnityEngine.Shader.PropertyToID("ditherScale");

        private static readonly int BlueNoise =
            UnityEngine.Shader.PropertyToID("_BlueNoise");

        private static readonly int BakedOpticalDepth =
            UnityEngine.Shader.PropertyToID("_BakedOpticalDepth");


        /// <summary>
        /// Sets all ordinary material parameters.
        /// The expensive optical-depth precomputation is handled by AtmospherePass.
        /// </summary>
        public void SetProperties(
            Material material,
            Vector3 planetCentre,
            float bodyRadius)
        {
            if (material == null)
                return;

            float atmosphereRadius =
                (1f + atmosphereScale) * bodyRadius;

            // Planet
            material.SetVector(
                "planetCentre",
                planetCentre
            );

            material.SetFloat(
                "planetRadius",
                bodyRadius
            );

            material.SetFloat(
                "oceanRadius",
                bodyRadius
            );

            material.SetFloat(
                "atmosphereRadius",
                atmosphereRadius
            );

            // Atmosphere
            material.SetInt(
                "numInScatteringPoints",
                inScatteringPoints
            );

            material.SetInt(
                "numOpticalDepthPoints",
                opticalDepthPoints
            );

            material.SetFloat(
                "densityFalloff",
                densityFalloff
            );

            float scatterX = Pow(400f / wavelengths.x, 4f);
            float scatterY = Pow(400f / wavelengths.y, 4f);
            float scatterZ = Pow(400f / wavelengths.z, 4f);

            material.SetVector(
                "scatteringCoefficients",
                new Vector3(scatterX, scatterY, scatterZ)
                * scatteringStrength
            );

            material.SetFloat("intensity", intensity);
            material.SetFloat("ditherStrength", ditherStrength);
            material.SetFloat("ditherScale", ditherScale);
            material.SetTexture("_BlueNoise", blueNoise);


            // Test-Sonne
            var sun = GameObject.Find("Test Sun");

            if (sun != null)
            {
                Vector3 sunPosition =
                    sun.transform.position;

                material.SetVector(
                    "dirToSun",
                    (sunPosition - planetCentre).normalized
                );
            }
        }


        public bool NeedsOpticalDepthPrecompute =>
            opticalDepthDirty ||
            opticalDepthTexture == null ||
            !opticalDepthTexture.IsCreated();


        /// <summary>
        /// Creates the persistent optical-depth RenderTexture if necessary.
        /// </summary>
        public RenderTexture GetOrCreateOpticalDepthTexture()
        {
            if (opticalDepthTexture != null &&
                opticalDepthTexture.IsCreated() &&
                opticalDepthTexture.width == textureSize &&
                opticalDepthTexture.height == textureSize)
            {
                return opticalDepthTexture;
            }

            ReleaseOpticalDepthTexture();

            opticalDepthTexture = new RenderTexture(
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

            opticalDepthTexture.Create();

            opticalDepthDirty = true;

            return opticalDepthTexture;
        }
        
        public RTHandle GetOpticalDepthHandle()
        {
            if (opticalDepthHandle == null)
            {
                RenderTexture texture =
                    GetOrCreateOpticalDepthTexture();

                opticalDepthHandle =
                    RTHandles.Alloc(texture);
            }

            return opticalDepthHandle;
        }


        /// <summary>
        /// Marks the precomputed optical depth as invalid.
        /// </summary>
        public void MarkOpticalDepthDirty()
        {
            opticalDepthDirty = true;
        }


        /// <summary>
        /// Called after the compute pass has been registered.
        /// </summary>
        public void MarkOpticalDepthClean()
        {
            opticalDepthDirty = false;
        }


        public void SetOpticalDepthTexture(Material material)
        {
            if (material == null)
                return;

            material.SetTexture(
                BakedOpticalDepth,
                opticalDepthTexture
            );
        }


        private void OnValidate()
        {
            textureSize = Max(1, textureSize);
            inScatteringPoints = Max(1, inScatteringPoints);
            opticalDepthPoints = Max(1, opticalDepthPoints);

            opticalDepthDirty = true;
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
            if (opticalDepthHandle != null)
            {
                opticalDepthHandle.Release();
                opticalDepthHandle = null;
            }

            if (opticalDepthTexture != null)
            {
                if (opticalDepthTexture.IsCreated())
                    opticalDepthTexture.Release();

                DestroyImmediate(opticalDepthTexture);
                opticalDepthTexture = null;
            }
        }
    }
}
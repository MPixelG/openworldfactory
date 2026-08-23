using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _Project.World.Atmosphere.Shader
{
    public class AtmosphereFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material material;

        [SerializeField] private AtmosphereSettings settings;

        private AtmospherePass _pass;


        public override void Create()
        {
            _pass = new AtmospherePass(
                material,
                settings
            )
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }


        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (material == null ||
                settings == null ||
                !settings.enabled)
            {
                return;
            }

            if (settings.opticalDepthCompute == null)
            {
                Debug.LogError(
                    "AtmosphereFeature: No optical depth compute shader assigned.",
                    this
                );

                return;
            }

            renderer.EnqueuePass(_pass);
        }


        protected override void Dispose(bool disposing)
        {
            _pass = null;
        }
    }
}
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _Project.World.Atmosphere.Shader
{
    public class AtmosphereFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Material material;

        [SerializeField]
        private AtmosphereSettings settings;

        private AtmospherePass pass;


        public override void Create()
        {
            pass = new AtmospherePass(
                material,
                settings
            );

            pass.renderPassEvent =
                RenderPassEvent.BeforeRenderingPostProcessing;
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

            renderer.EnqueuePass(pass);
        }


        protected override void Dispose(bool disposing)
        {
            //pass?.Dispose(); TODO dispose correctly
            pass = null;
        }
    }
}
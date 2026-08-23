using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal; 
// ReSharper disable InconsistentNaming

namespace _Project.World.Atmosphere.Shader
{
    public class AtmospherePass : ScriptableRenderPass
    {
        private readonly Material _material;
        private readonly AtmosphereSettings _settings;
        

        private static readonly int C_TextureSize =
            UnityEngine.Shader.PropertyToID("c_texture_size");
        

        private static readonly int C_AtmosphereRadius =
            UnityEngine.Shader.PropertyToID("c_atmosphere_radius");

        private static readonly int C_DensityFalloff =
            UnityEngine.Shader.PropertyToID("c_density_falloff");

        private static readonly int C_Result =
            UnityEngine.Shader.PropertyToID("c_result");


        private class ComputePassData
        {
            public ComputeShader Compute;
            public int Kernel;

            public int TextureSize;
            public float AtmosphereRadius;
            public float DensityFalloff;
            

            public TextureHandle Result;
        }


        public AtmospherePass(
            Material material,
            AtmosphereSettings settings)
        {
            _material = material;
            _settings = settings;
        }

 
        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            if (_material == null ||
                _settings == null ||
                _settings.opticalDepthCompute == null)
            {
                Debug.Log("ERROR IN RECORDING RENDER GRAPH FOR THE ATMOSPHERE: OpticalDepth");
                return;
            }

            var resourceData =
                frameData.Get<UniversalResourceData>();

            // We cannot sample the back buffer as a texture.
            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogWarning(
                    "AtmospherePass skipped because the active target is the back buffer."
                );

                return;
            }
            
            var atmosphere = AtmosphereController.Instance;

            if (atmosphere == null || _material == null)
            {
                return;
            }

            _settings.SetProperties(
                _material,
                atmosphere.PlanetCentre,
                atmosphere.planetRadius
            );
            


            TextureHandle opticalDepthHandle =
                renderGraph.ImportTexture(
                    _settings.GetOpticalDepthHandle()
                );
            

            if (_settings.NeedsOpticalDepthPrecompute)
            {
                Debug.Log("Needs optical depth precompute");
                AddOpticalDepthComputePass(
                    renderGraph,
                    opticalDepthHandle
                );

                _settings.MarkOpticalDepthClean(); 
            }


            _settings.SetOpticalDepthTexture(_material);

            
            TextureHandle source =
                resourceData.activeColorTexture;


            TextureDesc destinationDesc =
                renderGraph.GetTextureDesc(source);

            destinationDesc.name =
                "Atmosphere Camera Color";

            destinationDesc.clearBuffer = false;
            destinationDesc.depthBufferBits = 0;

            TextureHandle destination =
                renderGraph.CreateTexture(destinationDesc);


            AddFullscreenPass(
                renderGraph,
                source,
                destination,
                opticalDepthHandle
            );


            resourceData.cameraColor = destination;
        }


        private void AddOpticalDepthComputePass(
            RenderGraph renderGraph,
            TextureHandle opticalDepth)
        {
            int kernel =
                _settings.opticalDepthCompute.FindKernel("CSMain");


            using var builder =
                renderGraph.AddComputePass<ComputePassData>(
                    "Atmosphere Optical Depth",
                    out var passData
                );


            passData.Compute =
                _settings.opticalDepthCompute;

            passData.Kernel = kernel;

            passData.TextureSize =
                _settings.textureSize;

            passData.AtmosphereRadius =
                1f + _settings.atmosphereScale;

            passData.DensityFalloff =
                _settings.densityFalloff;

            passData.Result =
                opticalDepth;


            builder.UseTexture(
                opticalDepth,
                AccessFlags.Write
            );


            builder.SetRenderFunc(
                static (
                    ComputePassData data,
                    ComputeGraphContext context
                ) =>
                {
                    ComputeCommandBuffer cmd = context.cmd;


                    cmd.SetComputeIntParam(
                        data.Compute,
                        C_TextureSize,
                        data.TextureSize
                    );

                    cmd.SetComputeFloatParam(
                        data.Compute,
                        C_AtmosphereRadius,
                        data.AtmosphereRadius
                    );

                    cmd.SetComputeFloatParam(
                        data.Compute,
                        C_DensityFalloff,
                        data.DensityFalloff
                    );

                    cmd.SetComputeTextureParam(
                        data.Compute,
                        data.Kernel,
                        C_Result,
                        data.Result
                    );


                    cmd.DispatchCompute(
                        data.Compute,
                        data.Kernel,
                        8, 8, 1
                    );
                }
            );
        }


        private void AddFullscreenPass(
            RenderGraph renderGraph,
            TextureHandle source,
            TextureHandle destination,
            TextureHandle opticalDepth)
        {
            using var builder =
                renderGraph.AddRasterRenderPass<FullscreenPassData>(
                    "Atmosphere Fullscreen",
                    out var passData
                );


            passData.source = source;
            passData.material = _material;


            builder.UseTexture(
                source
            );

            
            builder.UseTexture(
                opticalDepth
            );


            builder.SetRenderAttachment(
                destination,
                0
            );


            builder.SetRenderFunc(
                static (
                    FullscreenPassData data,
                    RasterGraphContext context
                ) =>
                {
                    Blitter.BlitTexture(
                        context.cmd,
                        data.source,
                        new Vector4(1f, 1f, 0f, 0f),
                        data.material,
                        0
                    );
                }
            );
        }


        private class FullscreenPassData
        {
            public TextureHandle source;
            public TextureHandle opticalDepth;
            public Material material;
        }
    }
}
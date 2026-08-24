using System;
using Unity.Mathematics;
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
        

        private static readonly int CD_TextureSize =
            UnityEngine.Shader.PropertyToID("c_texture_size");
        

        private static readonly int CD_AtmosphereRadius =
            UnityEngine.Shader.PropertyToID("c_atmosphere_radius");

        private static readonly int CD_DensityFalloff =
            UnityEngine.Shader.PropertyToID("c_density_falloff");

        private static readonly int CD_Result =
            UnityEngine.Shader.PropertyToID("c_result");
        
        private static readonly int CS_TextureSize =
            UnityEngine.Shader.PropertyToID("cs_texture_size");
        

        private static readonly int CS_StarFrequency =
            UnityEngine.Shader.PropertyToID("cs_star_frequency");
        

        private static readonly int CS_Result =
            UnityEngine.Shader.PropertyToID("cs_result");


        private class OpticalDepthComputePassData
        {
            public ComputeShader Compute;
            public int Kernel;

            public int TextureSize;
            public float AtmosphereRadius;
            public float DensityFalloff;
            

            public TextureHandle Result;
        }
        
        private class StarMapComputePassData
        {
            public ComputeShader Compute;
            public int Kernel;

            public int TextureSize;
            public float starFrequency;
            

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
                _settings.opticalDepthCompute == null || _settings.starMapCompute == null)
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
                AddOpticalDepthComputePass(
                    renderGraph,
                    opticalDepthHandle
                );

                _settings.MarkOpticalDepthClean(); 
            }
            
            TextureHandle starMapHandle =
                renderGraph.ImportTexture(
                    _settings.GetStarMapHandle()
                );
            

            if (_settings.NeedsStarMapPrecompute)
            {
                AddStarMapComputePass(
                    renderGraph,
                    starMapHandle
                );

                _settings.MarkStarMapClean(); 
            }

            _settings.SetOpticalDepthTexture(_material);
            _settings.SetStarMapTexture(_material);

            
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


            AddFullscreenPasses(
                renderGraph,
                source,
                destination,
                opticalDepthHandle,
                starMapHandle
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
                renderGraph.AddComputePass<OpticalDepthComputePassData>(
                    "Atmosphere Optical Depth",
                    out var passData
                );


            passData.Compute =
                _settings.opticalDepthCompute;

            passData.Kernel = kernel;

            passData.TextureSize =
                _settings.opticalDepthTextureSize;

            passData.AtmosphereRadius =
                1f + _settings.atmosphereScale;

            passData.DensityFalloff =
                _settings.atmosphereDensityFalloff;

            passData.Result =
                opticalDepth;


            builder.UseTexture(
                opticalDepth,
                AccessFlags.Write
            );


            builder.SetRenderFunc(
                static (
                    OpticalDepthComputePassData data,
                    ComputeGraphContext context
                ) =>
                {
                    ComputeCommandBuffer cmd = context.cmd;


                    cmd.SetComputeIntParam(
                        data.Compute,
                        CD_TextureSize,
                        data.TextureSize
                    );

                    cmd.SetComputeFloatParam(
                        data.Compute,
                        CD_AtmosphereRadius,
                        data.AtmosphereRadius
                    );

                    cmd.SetComputeFloatParam(
                        data.Compute,
                        CD_DensityFalloff,
                        data.DensityFalloff
                    );

                    cmd.SetComputeTextureParam(
                        data.Compute,
                        data.Kernel,
                        CD_Result,
                        data.Result
                    );
                    int threadGroupCount = (int) math.ceil(CD_TextureSize / 8f);

                    cmd.DispatchCompute(
                        data.Compute,
                        data.Kernel,
                        threadGroupCount, threadGroupCount, 1
                    );
                }
            );
        }
        
        private void AddStarMapComputePass(
            RenderGraph renderGraph,
            TextureHandle starMap)
        {
            int kernel =
                _settings.starMapCompute.FindKernel("CSMain");


            using var builder =
                renderGraph.AddComputePass<StarMapComputePassData>(
                    "Atmosphere Star Map",
                    out var passData
                );


            passData.Compute =
                _settings.starMapCompute;

            passData.Kernel = kernel;

            passData.TextureSize =
                _settings.starMapTextureSize;

            passData.starFrequency =
                _settings.starFrequency;

            passData.Result =
                starMap;


            builder.UseTexture(
                starMap,
                AccessFlags.Write
            );


            builder.SetRenderFunc(
                static (
                    StarMapComputePassData data,
                    ComputeGraphContext context
                ) =>
                {
                    ComputeCommandBuffer cmd = context.cmd;


                    cmd.SetComputeIntParam(
                        data.Compute,
                        CS_TextureSize,
                        data.TextureSize
                    );

                    cmd.SetComputeFloatParam(
                        data.Compute,
                        CS_StarFrequency,
                        data.starFrequency
                    );

                    cmd.SetComputeTextureParam(
                        data.Compute,
                        data.Kernel,
                        CS_Result,
                        data.Result
                    );

                    int threadGroupCount = (int) math.ceil(data.TextureSize / 8f);
                    
                    
                    
                    
                    cmd.DispatchCompute(
                        data.Compute,
                        data.Kernel,
                        threadGroupCount, threadGroupCount, 1
                    );
                }
            );
        }


        private void AddFullscreenPasses(
            RenderGraph renderGraph,
            TextureHandle source,
            TextureHandle destination,
            TextureHandle opticalDepth,
            TextureHandle starMap
            )
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
            
            /*
            builder.UseTexture(
                starMap
            );*/


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
            //public TextureHandle starMap;
            public Material material;
        }
    }
}
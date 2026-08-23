using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace _Project.World.Atmosphere.Shader
{
    public class AtmospherePass : ScriptableRenderPass
    {
        private readonly Material material;
        private readonly AtmosphereSettings settings;
        

        private static readonly int TextureSize =
            UnityEngine.Shader.PropertyToID("textureSize");
        

        private static readonly int AtmosphereRadius =
            UnityEngine.Shader.PropertyToID("atmosphereRadius");

        private static readonly int DensityFalloff =
            UnityEngine.Shader.PropertyToID("densityFalloff");

        private static readonly int Result =
            UnityEngine.Shader.PropertyToID("Result");


        private class ComputePassData
        {
            public ComputeShader compute;
            public int kernel;

            public int textureSize;
            public int numOutScatteringSteps;
            public float atmosphereRadius;
            public float densityFalloff;
            

            public TextureHandle result;
        }


        public AtmospherePass(
            Material material,
            AtmosphereSettings settings)
        {
            this.material = material;
            this.settings = settings;
        }


        public void Dispose()
        {
            // AtmosphereSettings owns its persistent RenderTexture.
        }


        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            if (material == null ||
                settings == null ||
                settings.opticalDepthCompute == null)
            {
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


            // ------------------------------------------------------------
            // 1. Set normal material parameters
            // ------------------------------------------------------------
            
            var atmosphere = AtmosphereController.Instance;

            if (atmosphere == null || material == null)
                return;

            settings.SetProperties(
                material,
                atmosphere.PlanetCentre,
                atmosphere.planetRadius
            );


            // ------------------------------------------------------------
            // 2. Get/create persistent optical depth texture
            // ------------------------------------------------------------

            RenderTexture opticalDepth =
                settings.GetOrCreateOpticalDepthTexture();


            // Import the externally-owned RenderTexture into RenderGraph.
            TextureHandle opticalDepthHandle =
                renderGraph.ImportTexture(
                    settings.GetOpticalDepthHandle()
                );

            // ------------------------------------------------------------
            // 3. Recompute optical depth only when necessary
            // ------------------------------------------------------------

            if (settings.NeedsOpticalDepthPrecompute)
            {
                AddOpticalDepthComputePass(
                    renderGraph,
                    opticalDepthHandle
                );

                settings.MarkOpticalDepthClean();
            }


            // Make sure the material sees the persistent texture.
            settings.SetOpticalDepthTexture(material);


            // ------------------------------------------------------------
            // 4. Fullscreen atmosphere pass
            // ------------------------------------------------------------

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


            // Avoid an additional copy back to cameraColor.
            resourceData.cameraColor = destination;
        }


        private void AddOpticalDepthComputePass(
            RenderGraph renderGraph,
            TextureHandle opticalDepth)
        {
            int kernel =
                settings.opticalDepthCompute.FindKernel("CSMain");


            using var builder =
                renderGraph.AddComputePass<ComputePassData>(
                    "Atmosphere Optical Depth",
                    out var passData
                );


            passData.compute =
                settings.opticalDepthCompute;

            passData.kernel = kernel;

            passData.textureSize =
                settings.textureSize;

            passData.numOutScatteringSteps =
                settings.opticalDepthPoints;

            passData.atmosphereRadius =
                1f + settings.atmosphereScale;

            passData.densityFalloff =
                settings.densityFalloff;

            passData.result =
                opticalDepth;


            // The compute shader writes to this texture.
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
                    ComputeCommandBuffer cmd =
                        context.cmd;


                    cmd.SetComputeIntParam(
                        data.compute,
                        TextureSize,
                        data.textureSize
                    );

                    cmd.SetComputeFloatParam(
                        data.compute,
                        AtmosphereRadius,
                        data.atmosphereRadius
                    );

                    cmd.SetComputeFloatParam(
                        data.compute,
                        DensityFalloff,
                        data.densityFalloff
                    );

                    cmd.SetComputeTextureParam(
                        data.compute,
                        data.kernel,
                        Result,
                        data.result
                    );


                    data.compute.GetKernelThreadGroupSizes(
                        data.kernel,
                        out uint threadGroupX,
                        out uint threadGroupY,
                        out uint threadGroupZ
                    );


                    int groupsX =
                        Mathf.CeilToInt(
                            data.textureSize /
                            (float)threadGroupX
                        );

                    int groupsY =
                        Mathf.CeilToInt(
                            data.textureSize /
                            (float)threadGroupY
                        );


                    cmd.DispatchCompute(
                        data.compute,
                        data.kernel,
                        groupsX,
                        groupsY,
                        1
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
            passData.opticalDepth = opticalDepth;
            passData.material = material;


            // Camera color is read.
            builder.UseTexture(
                source,
                AccessFlags.Read
            );


            // Optical depth is read by the atmosphere shader.
            //
            // This is important: the RenderGraph does not know that
            // the material samples _BakedOpticalDepth just by inspecting
            // the Material. We explicitly declare the dependency here.
            builder.UseTexture(
                opticalDepth,
                AccessFlags.Read
            );


            // Destination becomes the render target.
            builder.SetRenderAttachment(
                destination,
                0,
                AccessFlags.Write
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
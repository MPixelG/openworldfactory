Shader "Custom/Atmosphere"
{
    SubShader
    {


        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Atmosphere"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "../../../Utils/Shaders/Math.cginc"
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(_BlueNoise);
            SAMPLER(sampler_BlueNoise);

            TEXTURE2D(_BakedOpticalDepth);
            SAMPLER(sampler_BakedOpticalDepth);


            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float2 uv = float2(
                    (input.vertexID << 1) & 2,
                    input.vertexID & 2
                );

                output.uv = uv;

                output.positionCS = float4(
                    uv * 2.0 - 1.0,
                    0.0,
                    1.0
                );

                return output;
            }

            float2 squareUV(float2 uv)
            {
                float width = _ScreenParams.x;
                float height = _ScreenParams.y;
                float scale = 1000;
                float x = uv.x * width;
                float y = uv.y * height;
                return float2(x / scale, y / scale);
            }

            float4 params;

            float3 dirToSun;

            float3 planetCentre;
            float atmosphereRadius;
            float oceanRadius;
            float planetRadius;

            // Paramaters
            #define NUM_IN_SCATTERING_POINTS 10
            #define NUM_OPTICAL_DEPTH_POINTS 10
            float intensity;
            float4 scatteringCoefficients;
            float ditherStrength;
            float ditherScale;
            float densityFalloff;


            float densityAtPoint(float3 densitySamplePoint)
            {
                float heightAboveSurface = length(densitySamplePoint - planetCentre) - oceanRadius;
                float height01 = heightAboveSurface / (atmosphereRadius - oceanRadius);
                float localDensity = exp(-height01 * densityFalloff) * (1 - height01);
                return localDensity;
            }

            float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 densitySamplePoint = rayOrigin;
                float stepSize = rayLength / (NUM_OPTICAL_DEPTH_POINTS - 1);
                float opticalDepth = 0;

                for (int i = 0; i < NUM_OPTICAL_DEPTH_POINTS; i++)
                {
                    float localDensity = densityAtPoint(densitySamplePoint);
                    opticalDepth += localDensity * stepSize;
                    densitySamplePoint += rayDir * stepSize;
                }
                return opticalDepth;
            }

            float opticalDepthBaked(float3 rayOrigin, float3 rayDir)
            {
                float height = length(rayOrigin - planetCentre) - oceanRadius;
                float height01 = saturate(height / (atmosphereRadius - oceanRadius));

                float uvX = 1 - (dot(normalize(rayOrigin - planetCentre), rayDir) * .5 + .5);
                return SAMPLE_TEXTURE2D_X(_BakedOpticalDepth, sampler_BakedOpticalDepth, float4(uvX, height01,0,0));
            }

            float opticalDepthBaked2(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 endPoint = rayOrigin + rayDir * rayLength;
                float d = dot(rayDir, normalize(rayOrigin - planetCentre));

                const float blendStrength = 1.5;
                float w = saturate(d * blendStrength + .5);

                float d1 = opticalDepthBaked(rayOrigin, rayDir) - opticalDepthBaked(endPoint, rayDir);
                float d2 = opticalDepthBaked(endPoint, -rayDir) - opticalDepthBaked(rayOrigin, -rayDir);

                float opticalDepth = lerp(d2, d1, w);
                return opticalDepth;
            }

            float3 calculate_light(float3 rayOrigin, float3 rayDir, float rayLength, float3 originalCol, float2 uv)
            {
                float blue_noise = SAMPLE_TEXTURE2D_X(_BlueNoise, sampler_BlueNoise,
                                                      float4(squareUV(uv) * ditherScale,0,0));
                blue_noise = (blue_noise - 0.5) * ditherStrength;

                float3 inScatterPoint = rayOrigin;
                float stepSize = rayLength / (NUM_IN_SCATTERING_POINTS - 1);
                float3 inScatteredLight = 0;
                float viewRayOpticalDepth = 0;

                for (int i = 0; i < NUM_IN_SCATTERING_POINTS; i++)
                {
                    float sunRayOpticalDepth = opticalDepthBaked(inScatterPoint + dirToSun * ditherStrength, dirToSun);
                    float localDensity = densityAtPoint(inScatterPoint);
                    viewRayOpticalDepth = opticalDepthBaked2(rayOrigin, rayDir, stepSize * i);
                    float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * scatteringCoefficients);

                    inScatteredLight += localDensity * transmittance;
                    inScatterPoint += rayDir * stepSize;
                }
                inScatteredLight *= scatteringCoefficients * intensity * stepSize / oceanRadius;
                inScatteredLight += blue_noise * 0.01;


                const float brightnessAdaptionStrength = 0.15;
                const float reflectedLightOutScatterStrength = 3;
                float brightnessAdaption = dot(inScatteredLight, 1) * brightnessAdaptionStrength;
                float brightnessSum = viewRayOpticalDepth * intensity * reflectedLightOutScatterStrength +
                    brightnessAdaption;
                float reflectedLightStrength = exp(-brightnessSum);
                float hdrStrength = saturate(dot(originalCol, 1) / 3 - 1);
                reflectedLightStrength = lerp(reflectedLightStrength, 1, hdrStrength);
                float3 reflectedLight = originalCol * reflectedLightStrength;

                float3 finalCol = reflectedLight + inScatteredLight;


                return finalCol;
            }


            half4 Frag(Varyings input) : SV_Target
            {
                half4 original_color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, input.uv);

                /*float scene_depth = SampleSceneDepth(input.uv);

                float linearDepth = LinearEyeDepth(
                    scene_depth,
                    _ZBufferParams
                ) / _ZBufferParams.y;*/
                 

                float scene_depth = SampleSceneDepth(input.uv);

                #if UNITY_REVERSED_Z
                    float depth = scene_depth;
                #else
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, scene_depth);
                #endif

                float3 sceneWorldPos =
                    ComputeWorldSpacePosition(
                        input.uv,
                        depth,
                        UNITY_MATRIX_I_VP
                    );
                float3 ray_origin = _WorldSpaceCameraPos;

                float2 ndc = input.uv * 2.0 - 1.0;

                float4 clipPos = float4(ndc, 1.0, 1.0);

                float4 worldPos =
                    mul(UNITY_MATRIX_I_VP, clipPos);

                worldPos.xyz /= worldPos.w;

                float3 ray_dir =
                    normalize(worldPos.xyz - ray_origin);


                float dstToScene = length(sceneWorldPos - ray_origin);

                float dstToOcean = raySphere(planetCentre, oceanRadius, ray_origin, ray_dir);
                
                //float scene_depth = SampleSceneDepth(input.uv);


                
                /*
                if (input.uv.x < 0.5)
                {
                    return dstToScene / 1000;
                } 
                return dstToOcean / 1000;
                */
                
                
                float dstToSurface = min(dstToScene, dstToOcean);

                float2 atmosphere_hit = raySphere(planetCentre, atmosphereRadius, ray_origin, ray_dir);
                float dstToAtmosphere = atmosphere_hit.x;
                float dstThroughAtmosphere = min(atmosphere_hit.y, dstToSurface - dstToAtmosphere);
                
                //return lerp((dstThroughAtmosphere) / 10000, original_color, 0.0);

                if (dstThroughAtmosphere > 0)
                {
                    const float epsilon = 0.0001;
                    float3 pointInAtmosphere = ray_origin + ray_dir * (dstToAtmosphere + epsilon);
                    float3 light = calculate_light(pointInAtmosphere, ray_dir, dstThroughAtmosphere - epsilon * 2,original_color, input.uv)/2;
                    float lightStrength = length(light);
                    return /*half4(light, 1.0);// */float4(light*lightStrength+original_color, 1.0);
                }
                return half4(original_color.rgb, 0.0);
            }
            ENDHLSL
        }
    }
}
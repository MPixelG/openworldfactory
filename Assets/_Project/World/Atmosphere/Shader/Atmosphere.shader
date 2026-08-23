Shader "Custom/Atmosphere" //todo credit seb
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
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "../../../Utils/Shaders/Math.cginc"
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(_BlueNoise);
            SAMPLER(sampler_BlueNoise);

            TEXTURE2D(_BakedOpticalDepth);
            SAMPLER(sampler_BakedOpticalDepth);


            struct attributes
            {
                uint vertex_id : SV_VertexID;
            };

            struct varyings
            {
                float4 position_cs : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            varyings vert(attributes input)
            {
                varyings output;

                float2 uv = float2(
                    (input.vertex_id << 1) & 2,
                    input.vertex_id & 2
                );

                output.uv = uv;

                output.position_cs = float4(
                    uv * 2.0 - 1.0,
                    0.0,
                    1.0
                );

                return output;
            }

            float2 square_uv(float2 uv)
            {
                float width = _ScreenParams.x;
                float height = _ScreenParams.y;
                float scale = 1000;
                float x = uv.x * width;
                float y = uv.y * height;
                return float2(x / scale, y / scale);
            }

            float3 dir_to_sun;

            float3 planet_center;
            float atmosphere_radius;
            float ocean_radius;
            float planet_radius;

            #define NUM_IN_SCATTERING_POINTS 10
            #define NUM_OPTICAL_DEPTH_POINTS 10
            float intensity;
            float4 scattering_coefficients;
            float dither_strength;
            float dither_scale;
            float density_falloff;


            float density_at_point(float3 density_sample_point)
            {
                float height_above_surface = length(density_sample_point - planet_center) - ocean_radius;
                float height01 = height_above_surface / (atmosphere_radius - ocean_radius);
                float local_density = exp(-height01 * density_falloff) * (1 - height01);
                return local_density;
            }

            float optical_depth(float3 ray_origin, float3 ray_dir, float ray_length)
            {
                float3 density_sample_point = ray_origin;
                float step_size = ray_length / (NUM_OPTICAL_DEPTH_POINTS - 1);
                float optical_depth = 0;

                for (int i = 0; i < NUM_OPTICAL_DEPTH_POINTS; i++)
                {
                    float local_density = density_at_point(density_sample_point);
                    optical_depth += local_density * step_size;
                    density_sample_point += ray_dir * step_size;
                }
                return optical_depth;
            }

            float optical_depth_baked(float3 ray_origin, float3 ray_dir)
            {
                float height = length(ray_origin - planet_center) - ocean_radius;
                float height01 = saturate(height / (atmosphere_radius - ocean_radius));

                float uv_x = 1 - (dot(normalize(ray_origin - planet_center), ray_dir) * .5 + .5);
                return SAMPLE_TEXTURE2D_X(_BakedOpticalDepth, sampler_BakedOpticalDepth, float4(uv_x, height01,0,0));
            } 

            float optical_depth_baked2(float3 ray_origin, float3 ray_dir, float ray_length)
            {
                float3 end_point = ray_origin + ray_dir * ray_length;
                float d = dot(ray_dir, normalize(ray_origin - planet_center));

                const float blend_strength = 1.5;
                float w = saturate(d * blend_strength + .5);

                float d1 = optical_depth_baked(ray_origin, ray_dir) - optical_depth_baked(end_point, ray_dir);
                float d2 = optical_depth_baked(end_point, -ray_dir) - optical_depth_baked(ray_origin, -ray_dir);

                float optical_depth = lerp(d2, d1, w);
                return optical_depth;
            }

            float3 calculate_light(float3 ray_origin, float3 ray_dir, float ray_length, float3 original_col, float2 uv)
            {
                float blue_noise = SAMPLE_TEXTURE2D_X(_BlueNoise, sampler_BlueNoise,
                                    float4(square_uv(uv) * dither_scale,0,0));
                blue_noise = (blue_noise - 0.5) * dither_strength;

                float3 in_scatter_point = ray_origin;
                float step_size = ray_length / (NUM_IN_SCATTERING_POINTS - 1);
                float3 in_scattered_light = 0;
                float view_ray_optical_depth = 0;

                for (int i = 0; i < NUM_IN_SCATTERING_POINTS; i++)
                {
                    float sun_ray_optical_depth = optical_depth_baked(in_scatter_point + dir_to_sun * dither_strength, dir_to_sun);
                    float local_density = density_at_point(in_scatter_point);
                    view_ray_optical_depth = optical_depth_baked2(ray_origin, ray_dir, step_size * i);
                    float3 transmittance = exp(-(sun_ray_optical_depth + view_ray_optical_depth) * scattering_coefficients);

                    in_scattered_light += local_density * transmittance;
                    in_scatter_point += ray_dir * step_size;
                }
                in_scattered_light *= scattering_coefficients * intensity * step_size / ocean_radius;
                in_scattered_light += blue_noise * 0.01;


                const float brightness_adaption_strength = 0.15;
                const float reflected_light_out_scatter_strength = 3;
                float brightness_adaption = dot(in_scattered_light, 1) * brightness_adaption_strength;
                float brightness_sum = view_ray_optical_depth * intensity * reflected_light_out_scatter_strength + brightness_adaption;
                float reflected_light_strength = exp(-brightness_sum);
                float hdr_strength = saturate(dot(original_col, 1) / 3 - 1);
                reflected_light_strength = lerp(reflected_light_strength, 1, hdr_strength);
                float3 reflected_light = original_col * reflected_light_strength;

                float3 final_col = reflected_light + in_scattered_light;


                return final_col;
            }


            half4 frag(varyings input) : SV_Target
            {
                half4 original_color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, input.uv);

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

                float4 clip_pos = float4(ndc, 1.0, 1.0);

                float4 world_pos =
                    mul(UNITY_MATRIX_I_VP, clip_pos);

                world_pos.xyz /= world_pos.w;

                float3 ray_dir =
                    normalize(world_pos.xyz - ray_origin);


                float dst_to_scene = length(sceneWorldPos - ray_origin);

                float2 atmosphere_hit = raySphere(planet_center, atmosphere_radius, ray_origin, ray_dir);
                float dst_to_atmosphere = atmosphere_hit.x;
                float dst_through_atmosphere = min(atmosphere_hit.y, dst_to_scene - dst_to_atmosphere);


                if (dst_through_atmosphere > 0)
                {
					const float epsilon = 0.0001;
					float3 point_in_atmosphere = ray_origin + ray_dir * (dst_to_atmosphere + epsilon);
					float3 light = calculate_light(point_in_atmosphere, ray_dir, dst_through_atmosphere - epsilon * 2, original_color, input.uv);
                    return float4(light, 1);
                }
                return half4(original_color.rgb, 0.0);
            }
            ENDHLSL
        }
    }
}
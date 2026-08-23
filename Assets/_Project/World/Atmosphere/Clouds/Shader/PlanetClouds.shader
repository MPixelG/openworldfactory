Shader "Planet/VolumetricClouds"
{
    Properties
    {
        _RInner ("Deck Bottom Radius", Float) = 165
        _ROuter ("Deck Top Radius",    Float) = 182
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Clouds"
            Blend One SrcAlpha   // the shader does the following -> result = luminance × 1 + background × transmittance
            ZWrite Off // no single depth available because of transparency and nature of clouds -> no distinct z value
            ZTest  Always // ZTest (Depth testing) compares the depth of all objects on that pixel and draws the one closest to the camera if active
            Cull   Front // prohibits that the volumetrics vanish when you enter the cloud shell

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma target   4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial) //batching order, future properties need to be added here later todo
                float _RInner;
                float _ROuter;
            CBUFFER_END

            struct attributes
            {
                float4 positionOS : POSITION;
            };

            struct varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            varyings Vert(attributes IN)
            {
                varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                return OUT;
            }

            half4 Frag(varyings IN) : SV_Target
            {
                // rgb = light we add, a = fraction of the background we keep
                return half4(0.0, 0.0, 0.0, 0.5);
            }
            ENDHLSL
        }
    }
}
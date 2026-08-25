Shader "Planet/VolumetricClouds"

{
    Properties
    {
        _RInner ("Deck Bottom Radius", Float) = 165
        _ROuter ("Deck Top Radius",    Float) = 182
        _PlanetCenter("Center of Planet", Vector) = (0,0,0,0)
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
                float4 _PlanetCenter;
            CBUFFER_END
            
            #include "PlanetCloudsCommon.hlsl"

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
            float3 ro = _WorldSpaceCameraPos - _PlanetCenter.xyz;
            float3 rd = normalize(IN.positionWS - _WorldSpaceCameraPos);
            float2 seg = RayShell(ro, rd, _RInner, _ROuter);
            if (seg.y <= seg.x) discard;

            // debug: how far does this ray travel through the deck?
            float len = (seg.y - seg.x) / (_ROuter - _RInner);
            return half4(len.xxx * 0.25, 0.0);
            }
            ENDHLSL
        }
    }
}
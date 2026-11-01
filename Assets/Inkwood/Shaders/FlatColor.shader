Shader "Inkwood/Toon Color"
{
    Properties
    {
        [MainColor] _BaseColor("Lit color", Color) = (1,1,1,1)
        _ShadowColor("Shadow color", Color) = (0.5,0.6,0.6,1)
        _BandThreshold("Lit / shadow boundary", Range(-1,1)) = 0.25
        _BandSoftness("Band edge softness", Range(0.001,0.15)) = 0.015
        [Toggle] _LightingEnabled("Directional toon lighting", Float) = 1
        [Toggle] _ReceiveShadows("Receive cast shadows", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        half4 _BaseColor;
        half4 _ShadowColor;
        float _BandThreshold;
        float _BandSoftness;
        float _LightingEnabled;
        float _ReceiveShadows;
        CBUFFER_END
        ENDHLSL
        Pass
        {
            Name "ToonForward"
            Tags { "LightMode"="UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 positionWS:TEXCOORD1; };
            Varyings vert(Attributes v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                Varyings o;
                o.positionWS=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float n=dot(normalize(i.normalWS),sun.direction);
                float edge=max(_BandSoftness,fwidth(n));
                float band=smoothstep(_BandThreshold-edge,_BandThreshold+edge,n);
                float lit=lerp(1.0,band,_LightingEnabled);
                lit*=lerp(1.0,sun.shadowAttenuation,_ReceiveShadows);
                half3 color=lerp(_ShadowColor.rgb,_BaseColor.rgb,lit);
                color*=lerp(half3(1,1,1),sun.color,_LightingEnabled);
                return half4(color,1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}

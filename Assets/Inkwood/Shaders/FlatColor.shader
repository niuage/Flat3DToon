Shader "Inkwood/Flat Color"
{
    Properties
    {
        _BaseColor("Color", Color) = (1,1,1,1)
        _ShadeStrength("Subtle shading", Range(0,0.3)) = 0.10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float _ShadeStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                float n=dot(normalize(i.normalWS), normalize(float3(-0.4,0.85,-0.3)));
                float band=n>0.3?1.0:(n> -0.3?0.55:0.0);
                return half4(_BaseColor.rgb*(1.0-_ShadeStrength*(1.0-band)),1);
            }
            ENDHLSL
        }
    }
}

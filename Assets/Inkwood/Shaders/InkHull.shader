Shader "Inkwood/Ink Outline"
{
    Properties
    {
        _BaseColor("Ink color", Color)=(0.20,0.14,0.12,1)
        _OutlinePixels("Outline width at 1080p (pixels)", Range(0,8))=3.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry-1" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float _OutlinePixels;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            float4 vert(Attributes v):SV_POSITION
            {
                float3 p=TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS=TransformObjectToWorldNormal(v.normalOS);
                float viewHeight=2.0/max(abs(UNITY_MATRIX_P._m11),0.0001);
                if(UNITY_MATRIX_P._m33<0.5) viewHeight*=abs(TransformWorldToView(p).z);
                // Extrude the actual surface: screen-space offsets can invert small cap triangles.
                p+=normalWS*(_OutlinePixels*viewHeight/1080.0);
                return TransformWorldToHClip(p);
            }
            half4 frag():SV_Target { return _BaseColor; }
            ENDHLSL
        }
    }
}

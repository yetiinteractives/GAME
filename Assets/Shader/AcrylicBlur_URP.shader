Shader "Custom/UI/AcrylicBlue_URP"
{
    Properties
    {
        _BlurTex ("Blur Tex", 2D) = "black" {}
        _TintColor ("Tint", Color) = (0.85,0.9,1,0.35)
        _NoiseTex ("Noise", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.04
        _NoiseTiling ("Noise Tiling", Float) = 8
        _Saturation ("Saturation", Range(0,2)) = 1
        _Luminosity ("Luminosity", Range(0,2)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "UIAcrylic"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_UIBlurTexture);
            SAMPLER(sampler_UIBlurTexture);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _TintColor;
            float _NoiseStrength;
            float _NoiseTiling;
            float _Saturation;
            float _Luminosity;
            float4 _NoiseTex_ST;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionHCS = p.positionCS;
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.screenPos = ComputeScreenPos(o.positionHCS);
                return o;
            }

            float3 SaturateColor(float3 c, float s)
            {
                float l = dot(c, float3(0.299, 0.587, 0.114));
                return lerp(l.xxx, c, s);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 suv = i.screenPos.xy / i.screenPos.w;
                float3 col = SAMPLE_TEXTURE2D(_UIBlurTexture, sampler_UIBlurTexture, suv).rgb;

                col *= _Luminosity;
                col = SaturateColor(col, _Saturation);

                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv * _NoiseTiling).r * 2 - 1;
                col += noise * _NoiseStrength;

                col = lerp(col, _TintColor.rgb, _TintColor.a);

                return half4(col, _TintColor.a);
            }
            ENDHLSL
        }
    }
}
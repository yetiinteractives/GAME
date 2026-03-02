Shader "Custom/UI/KawaseBlur"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "KawaseBlur"
            ZWrite Off ZTest Always Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_LinearClamp);
            float _BlurOffset;

            half4 Frag (Varyings i) : SV_Target
            {
                float2 texel = _BlitTexture_TexelSize.xy * _BlurOffset;
                float2 uv = i.texcoord;

                half3 c = 0;
                c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( texel.x,  texel.y)).rgb;
                c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-texel.x,  texel.y)).rgb;
                c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( texel.x, -texel.y)).rgb;
                c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-texel.x, -texel.y)).rgb;
                c *= 0.25;

                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
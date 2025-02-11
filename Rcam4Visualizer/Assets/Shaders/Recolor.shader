Shader "Hidden/Rcam4/Recolor"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_SourceTexture);
TEXTURE2D(_AlphaTexture);

float4 FragCapture(float4 position : SV_Position) : SV_Target0
{
    return LOAD_TEXTURE2D(_BlitTexture, position.xy).a;
}

float4 FragRecolor(float4 position : SV_Position) : SV_Target0
{
    float3 rgb = LOAD_TEXTURE2D(_SourceTexture, position.xy).rgb;
    float a = LOAD_TEXTURE2D(_AlphaTexture, position.xy).r;
    return float4(lerp(0, rgb, 0.5 + 0.5 * a), 1);
}

ENDHLSL

    SubShader
    {
        ZTest Off ZWrite Off Cull Off Blend Off
        Pass
        {
            Name "Recolor Capture"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCapture
            ENDHLSL
        }
        Pass
        {
            Name "Recolor"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragRecolor
            ENDHLSL
        }
    }
}

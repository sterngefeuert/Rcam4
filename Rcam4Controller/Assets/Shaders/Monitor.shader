Shader "Rcam4/Monitor"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/jp.keijiro.rcam4.common/Shaders/RcamCommon.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float2 _DepthRange;
float4 _InverseProjection;
float4x4 _InverseView;

// Source texture sampler with 0.5 pixel boundary guard
float4 SampleSource(float2 uv, float2 scale, float2 offset)
{
    uint w, h;
    _MainTex.GetDimensions(w, h);

    float2 tsize = 0.5 / (scale * float2(w, h));
    uv = clamp(uv, tsize, 1 - tsize);

    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv * scale + offset);
}

void Vertex(uint vertexID : VERTEXID_SEMANTIC,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

float4 Fragment(float4 position : SV_Position,
                float2 texCoord : TEXCOORD) : SV_Target
{
    // Samples
    float4 s_color = SampleSource(texCoord, float2(0.5, 1.0), 0);
    float4 s_depth = SampleSource(texCoord, 0.5, float2(0.5, 0.5));
    float4 s_mask  = SampleSource(texCoord, 0.5, float2(0.5, 0.0));

    // Information
    float3 color = LinearToSRGB(s_color.rgb);
    float depth = RcamDecodeDepth(LinearToSRGB(s_depth.rgb), _DepthRange);
    float human = s_mask.r;
    float conf = s_mask.g;

    // Inverse projection into the world space
    float3 wpos = RcamDistanceToWorldPosition
      (texCoord, depth, _InverseProjection, _InverseView);
    float3 wnrm = normalize(fwidth(wpos));
    float3 wmask = smoothstep(0.6, 0.7, wnrm);

    // Grid lines
    float3 grid3 = smoothstep(0.05, 0, abs(0.5 - frac(wpos * 20))) * wmask;
    float grid = max(grid3.x, max(grid3.y, grid3.z));
    grid *= smoothstep(_DepthRange.x, _DepthRange.x + 0.1, depth);
    grid *= smoothstep(_DepthRange.y, _DepthRange.y - 0.1, depth);
    grid *= smoothstep(0.9, 1, conf);

    // Human contour
    float cont = smoothstep(0, 0.2, fwidth(human));

    // Output blending
    float3 srgb = lerp(color, float3(1, 1, 1), 0.5 * grid);
    srgb = lerp(srgb, float3(0, 1, 0), 0.5 * cont);
    return float4(SRGBToLinear(srgb), 1);
}

ENDHLSL

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }

    Fallback Off
}

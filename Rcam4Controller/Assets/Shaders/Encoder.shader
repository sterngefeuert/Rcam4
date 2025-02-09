Shader "Hidden/Rcam4/Frame Encoder"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/jp.keijiro.rcam4.common/Shaders/RcamCommon.hlsl"

sampler2D _textureY;
sampler2D _textureCbCr;
sampler2D _HumanStencil;
sampler2D _EnvironmentDepth;
sampler2D _EnvironmentDepthConfidence;
float2 _DepthRange;

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
    float4 tc = frac(texCoord.xyxy * float4(1, 1, 2, 2));

    // Vertical flip
    tc.yw = 1 - tc.yw;

    // Texture samples
    float y = tex2D(_textureY, tc.zy).x;
    float2 cbcr = tex2D(_textureCbCr, tc.zy).xy;
    float depth = tex2D(_EnvironmentDepth, tc.zw).x;
    float mask = tex2D(_HumanStencil, tc.zw).x;
    float score = tex2D(_EnvironmentDepthConfidence, tc.zw).x;

    // Color plane
    float3 c1 = RcamYCbCrToSRGB(y, cbcr);

    // Depth plane
    float3 c2 = RcamEncodeDepth(depth, _DepthRange);

    // Mask plane
    float3 c3 = float3(mask, score * 128, 0);

    // Output
    float3 srgb = tc.x < 0.5 ? c1 : (tc.y < 0.5 ? c2 : c3);

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

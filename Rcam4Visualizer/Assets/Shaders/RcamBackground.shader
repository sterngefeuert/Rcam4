Shader "Hidden/Rcam4/Background"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/jp.keijiro.rcam4.common/Shaders/RcamCommon.hlsl"

sampler2D _ColorMap;
sampler2D _DepthMap;
float4 _InverseProjection;
float4x4 _InverseView;

void Vertex(uint vertexID : VERTEXID_SEMANTIC,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

void Fragment(float4 position : SV_Position,
              float2 texCoord : TEXCOORD,
              out float4 outColor : SV_Target,
              out float outDepth : SV_Depth)
{
    // Aspect ratio fix
    float inv_aspect = _InverseProjection.y / _InverseProjection.x;
    float2 uv = texCoord * 2 - 1;
    uv = (uv * float2(1, inv_aspect));
    uv = (uv + 1) / 2;

    // Color/depth samples
    float4 c = tex2D(_ColorMap, uv);
    float d = tex2D(_DepthMap, uv).x;

    // Human stencil
    float a = c.a > 0.51;

    // Output
    outColor = float4(c.rgb, a);
    outDepth = RcamDistanceToDepth(d);
}

ENDHLSL

    SubShader
    {
        Pass
        {
            ZTest LEqual ZWrite On Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }

    Fallback Off
}

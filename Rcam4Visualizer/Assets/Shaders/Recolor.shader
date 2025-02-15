Shader "Hidden/Rcam4/Recolor"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

// Shader properties
TEXTURE2D(_SourceTexture);
TEXTURE2D(_AlphaTexture);
float4x4 _BgColors, _FgColors;
float4 _ContourColor;
float2 _ContourThresh;
float _Dithering;

// Bayer matrix for dithering
static const float BayerArray[] =
{
    0.000000, 0.531250, 0.132812, 0.664062,
    0.796875, 0.265625, 0.929688, 0.398438,
    0.199219, 0.730469, 0.066406, 0.597656,
    0.996094, 0.464844, 0.863281, 0.332031,
};

static float GetDither(uint2 tc)
{
    return BayerArray[(tc.x & 3) + (tc.y & 3) * 4];
}

// Applying a palette stored in a float4x4 matrix
float3 ApplyPalette(float4x4 palette, float lum)
{
    float3 c = palette[0].rgb;
    c = lum > palette[1].w ? palette[1].rgb : c;
    c = lum > palette[2].w ? palette[2].rgb : c;
    c = lum > palette[3].w ? palette[3].rgb : c;
    return c;
}

// Fragment shader: Capture pass (alpha copy)
float4 FragCapture(float4 position : SV_Position) : SV_Target0
{
    return LOAD_TEXTURE2D(_BlitTexture, position.xy).a;
}

// Fragment shader: Recolor pass
float4 FragRecolor(float4 position : SV_Position) : SV_Target0
{
    // Source texture dimensions
    uint w, h;
    _SourceTexture.GetDimensions(w, h);

    // Texture coordinates
    uint2 tc0 = position.xy;
    uint2 tc1 = uint2(min(tc0.x + 1, w - 1), tc0.y);
    uint2 tc2 = uint2(tc0.x, min(tc0.y + 1, h - 1));
    uint2 tc3 = uint2(tc1.x, tc2.y);

    // Color/alpha samples
    float3 c0 = LOAD_TEXTURE2D(_SourceTexture, tc0).rgb;
    float3 c1 = LOAD_TEXTURE2D(_SourceTexture, tc1).rgb;
    float3 c2 = LOAD_TEXTURE2D(_SourceTexture, tc2).rgb;
    float3 c3 = LOAD_TEXTURE2D(_SourceTexture, tc3).rgb;
    float a = LOAD_TEXTURE2D(_AlphaTexture, tc0).r; 

    // Luminance values
    float l0 = Luminance(LinearToSRGB(c0));
    float l1 = Luminance(LinearToSRGB(c1));
    float l2 = Luminance(LinearToSRGB(c2));
    float l3 = Luminance(LinearToSRGB(c3));

    // Edge detection with Roberts cross operator
    float g1 = l1 - l0;
    float g2 = l3 - l2;
    float g = sqrt(g1 * g1 + g2 * g2);
    float cont = smoothstep(_ContourThresh.x, _ContourThresh.y, g);

    // Dithering
    float lum = l0 + (GetDither(tc0) - 0.5) * _Dithering;

    // Composition
    float3 bg = ApplyPalette(_BgColors, lum);
    float3 fg = ApplyPalette(_FgColors, lum);
    fg = lerp(fg, _ContourColor.rgb, _ContourColor.a * cont);
    return float4(lerp(bg, fg, a), 1);
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

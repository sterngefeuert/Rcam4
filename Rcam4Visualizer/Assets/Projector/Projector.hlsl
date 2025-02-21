#include "Packages/jp.keijiro.noiseshader/Shader/Noise1D.hlsl"
#include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise2D.hlsl"

void ColorSampleWithBlackOut_float
  (UnityTexture2D Source,
   float2 UV,
   float BlackOut,
   out float3 Output)
{
    float3 srgb = LinearToSRGB(tex2D(Source, UV).rgb) * (1 - BlackOut);
    Output = SRGBToLinear(srgb);
}

void OverlaySoftEdge_float
  (UnityTexture2D RefTexture,
   float2 UV,
   float BorderWidth,
   out float Output)
{
    float2 uv = min(float2(UV.x, 0.5), 1 - UV);
    uv.x *= RefTexture.texelSize.z * RefTexture.texelSize.y;
    float dist = length(saturate(1 - uv / BorderWidth));
    Output = 1 - dist * dist;
}

void Background_float
  (UnityTexture2D Source,
   float2 UV,
   float2 Frequency,
   float NoiseAmp,
   float BlackOut,
   out float3 Output)
{
    // Noise animation
    float2 coord1 = float2(UV.x, _Time.y) * Frequency;
    float2 coord2 = coord1 * 3 + 1000;
    float n = SimplexNoise(coord1) + SimplexNoise(coord2);

    // Composition
    float amp = saturate(lerp(-1, 2, NoiseAmp) + clamp(n, -1, 1));
    float3 srgb = LinearToSRGB(tex2D(Source, UV).rgb);
    srgb *= amp * (1 - BlackOut);

    Output = SRGBToLinear(srgb);
}

using UnityEngine;

namespace Rcam4 {

static class ShaderID
{
    public static readonly int ColorMap = Shader.PropertyToID("_ColorMap");
    public static readonly int DepthMap = Shader.PropertyToID("_DepthMap");
    public static readonly int DepthRange = Shader.PropertyToID("_DepthRange");
    public static readonly int InverseProjection = Shader.PropertyToID("_InverseProjection");
    public static readonly int InverseView = Shader.PropertyToID("_InverseView");
    public static readonly int LutTexture = Shader.PropertyToID("_LutTexture");
}

// Simple replacement of Graphics.Blit
class Blitter : System.IDisposable
{
    Material _material;

    public Material Material => _material;

    public Blitter(Shader shader)
      => _material = new Material(shader);

    public void Run(Texture source, RenderTexture dest, int pass)
    {
        RenderTexture.active = dest;
        _material.mainTexture = source;
        _material.SetPass(pass);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 3, 1);
    }

    public void Dispose()
    {
        if (_material != null) Object.Destroy(_material);
    }
}

// Render texture allocation utility
static class RTUtil
{
    public static RenderTexture AllocColor(int width, int height)
      => new RenderTexture(width, height, 0)
           { wrapMode = TextureWrapMode.Clamp };

    public static RenderTexture AllocHalf(int width, int height)
      => new RenderTexture(width, height, 0, RenderTextureFormat.RHalf)
           { wrapMode = TextureWrapMode.Clamp };
}

} // namespace Rcam4

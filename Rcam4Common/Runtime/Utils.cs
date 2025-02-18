using UnityEngine;

namespace Rcam4 {

// Shader property IDs
public static class ShaderID
{
    public static readonly int AlphaTexture = Shader.PropertyToID("_AlphaTexture");
    public static readonly int ColorMap = Shader.PropertyToID("_ColorMap");
    public static readonly int DepthMap = Shader.PropertyToID("_DepthMap");
    public static readonly int DepthRange = Shader.PropertyToID("_DepthRange");
    public static readonly int EnvironmentDepth = Shader.PropertyToID("_EnvironmentDepth");
    public static readonly int EnvironmentDepthConfidence = Shader.PropertyToID("_EnvironmentDepthConfidence");
    public static readonly int HumanStencil = Shader.PropertyToID("_HumanStencil");
    public static readonly int InverseProjection = Shader.PropertyToID("_InverseProjection");
    public static readonly int InverseView = Shader.PropertyToID("_InverseView");
    public static readonly int LutTexture = Shader.PropertyToID("_LutTexture");
    public static readonly int SourceTexture = Shader.PropertyToID("_SourceTexture");
    public static readonly int TextureCbCr = Shader.PropertyToID("_textureCbCr");
    public static readonly int TextureY = Shader.PropertyToID("_textureY");
}

// Camera parameter utility
public static class CameraUtil
{
    public static Vector4 GetInverseProjection(in Metadata md)
    {
        var x = 1 / md.ProjectionMatrix[0, 0];
        var y = 1 / md.ProjectionMatrix[1, 1];
        var z = md.ProjectionMatrix[0, 2] * x;
        var w = md.ProjectionMatrix[1, 2] * y;
        return new Vector4(x, y, z, w);
    }

    public static Matrix4x4 GetInverseView(in Metadata md)
      => md.CameraPosition == Vector3.zero ? Matrix4x4.identity :
         Matrix4x4.TRS(md.CameraPosition, md.CameraRotation, Vector3.one);

    public static float GetFieldOfView(in Metadata md)
      => Mathf.Rad2Deg * Mathf.Atan(1 / md.ProjectionMatrix[1, 1]) * 2;
}

// Render texture allocation utility
public static class RTUtil
{
    public static RenderTexture AllocColor(int width, int height)
      => new RenderTexture(width, height, 0)
           { wrapMode = TextureWrapMode.Clamp };

    public static RenderTexture AllocHalf(int width, int height)
      => new RenderTexture(width, height, 0, RenderTextureFormat.RHalf)
           { wrapMode = TextureWrapMode.Clamp };
}

// Replacement of Graphics.Blit
public class Blitter : System.IDisposable
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

} // namespace Rcam4

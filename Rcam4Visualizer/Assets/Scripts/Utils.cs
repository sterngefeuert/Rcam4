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

static class ComputeShaderExtensions
{
    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y = 1, int z = 1)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);
        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;
        compute.Dispatch(kernel, x, y, z);
    }
}

} // namespace Rcam4

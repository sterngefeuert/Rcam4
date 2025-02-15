using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

// Capture Pass: Captures the alpha channel of the current frame buffer.
// Recolor Pass: Applies the posterization effect using the captured alpha.

namespace Rcam4 {

sealed class RecolorContextData : ContextItem
{
    public TextureHandle AlphaBuffer { get; set; }

    public override void Reset()
      => AlphaBuffer = TextureHandle.nullHandle;
}

sealed class RecolorCapturePass : ScriptableRenderPass
{
    Material _material;

    public RecolorCapturePass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<RecolorEffect>();
        if (driver == null || !driver.enabled || !driver.IsActive) return;

        // Destination texture allocation
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);
        desc.name = "Alpha Bypass";
        desc.colorFormat = GraphicsFormat.R8_UNorm;
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        var buffer = graph.CreateTexture(desc);

        // Custom context data for transferring the texture
        var contextData = context.Create<RecolorContextData>();
        contextData.AlphaBuffer = buffer;

        // Blit
        var param = new RenderGraphUtils.
          BlitMaterialParameters(source, buffer, _material, 0);
        graph.AddBlitPass(param, passName: "Recolor (capture alpha)");
    }
}

sealed class RecolorEffectPass : ScriptableRenderPass
{
    class PassData
    {
        public Material Material;
        public RecolorEffect Driver;
        public TextureHandle Source;
        public TextureHandle Alpha;
    }

    Material _material;

    public RecolorEffectPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<RecolorEffect>();
        if (driver == null || !driver.enabled || !driver.IsActive) return;

        // Custom context data retrieval
        var contextData = context.Get<RecolorContextData>();

        // Destination texture allocation
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);
        desc.name = "Recolor";
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        var dest = graph.CreateTexture(desc);

        // Render pass building
        using var builder = graph.
          AddRasterRenderPass<PassData>("Recolor", out var data);

        // Custom pass data
        data.Material = _material;
        data.Driver = driver;
        data.Source = source;
        data.Alpha = contextData.AlphaBuffer;

        // Texture registration
        builder.UseTexture(data.Source);
        builder.UseTexture(data.Alpha);

        // Color attachment
        builder.SetRenderAttachment(dest, 0);

        // Render function registration
        builder.SetRenderFunc<PassData>((data, ctx) => ExecutePass(data, ctx));

        // Destination texture as the camera texture
        resource.cameraColor = dest;
    }

    static void ExecutePass(PassData data, RasterGraphContext ctx)
    {
        data.Material.SetTexture(ShaderID.SourceTexture, data.Source);
        data.Material.SetTexture(ShaderID.AlphaTexture, data.Alpha);
        CoreUtils.DrawFullScreen(ctx.cmd, data.Material, data.Driver.Properties, 1);
    }
}

public sealed class RecolorFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    RecolorCapturePass _capturePass;
    RecolorEffectPass _effectPass;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _capturePass = new RecolorCapturePass(_material);
        _effectPass = new RecolorEffectPass(_material);
        _capturePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        _effectPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    protected override void Dispose(bool disposing)
      => CoreUtils.Destroy(_material);

    public override void AddRenderPasses
      (ScriptableRenderer renderer, ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_capturePass);
        renderer.EnqueuePass(_effectPass);
    }
}

} // namespace Rcam4

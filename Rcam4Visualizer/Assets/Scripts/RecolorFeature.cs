using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

// Capture Pass: Captures the alpha channel of the current frame buffer.
// Recolor Pass: Applies the posterization effect using the captured alpha.

namespace Rcam4 {

sealed class CapturePass : ScriptableRenderPass
{
    Material _material;

    public TextureHandle Buffer { get; set; }

    public CapturePass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Destination texture allocation
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);
        desc.name = "Alpha Bypass";
        desc.colorFormat = GraphicsFormat.R8_UNorm;
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        Buffer = graph.CreateTexture(desc);

        // Blit
        var param = new RenderGraphUtils.
          BlitMaterialParameters(source, Buffer, _material, 0);
        graph.AddBlitPass(param, passName: "Recolor (capture alpha)");
    }
}

sealed class RecolorPass : ScriptableRenderPass
{
    class PassData
    {
        public Material Material;
        public Recolor Driver;
        public TextureHandle Source;
        public TextureHandle Buffer;
    }

    Material _material;
    CapturePass _capture;

    public RecolorPass(Material material, CapturePass capture)
      => (_material, _capture) = (material, capture);

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<Recolor>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;

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
        data.Buffer = _capture.Buffer;

        // Texture registration
        builder.UseTexture(source);
        builder.UseTexture(_capture.Buffer);

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
        data.Material.SetTexture(ShaderID.AlphaTexture, data.Buffer);
        CoreUtils.DrawFullScreen(ctx.cmd, data.Material, data.Driver.Properties, 1);
    }
}

public sealed class RecolorFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    CapturePass _capture;
    RecolorPass _recolor;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _capture = new CapturePass(_material);
        _recolor = new RecolorPass(_material, _capture);
        _capture.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        _recolor.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    protected override void Dispose(bool disposing)
      => CoreUtils.Destroy(_material);

    public override void AddRenderPasses
      (ScriptableRenderer renderer, ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_capture);
        renderer.EnqueuePass(_recolor);
    }
}

} // namespace Rcam4

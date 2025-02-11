using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Rcam4 {

sealed class RcamBackgroundPass : ScriptableRenderPass
{
    class PassData
    {
        public Material Material;
        public RcamBackground Driver;
    }

    Material _material;

    public RcamBackgroundPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<RcamBackground>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;

        // Render pass building
        using var builder = graph.
          AddRasterRenderPass<PassData>("Rcam BG", out var data);

        // Custom pass data
        data.Material = _material;
        data.Driver = driver;

        // Color/depth attachments
        var resource = context.Get<UniversalResourceData>();
        builder.SetRenderAttachment(resource.activeColorTexture, 0);
        builder.SetRenderAttachmentDepth
          (resource.activeDepthTexture, AccessFlags.Write);

        // Render function registration
        builder.SetRenderFunc<PassData>((data, ctx) => ExecutePass(data, ctx));
    }

    static void ExecutePass(PassData data, RasterGraphContext ctx)
      => CoreUtils.DrawFullScreen(ctx.cmd, data.Material, data.Driver.Properties);
}

public sealed class RcamBackgroundFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    RcamBackgroundPass _pass;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _pass = new RcamBackgroundPass(_material);
        _pass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses
      (ScriptableRenderer renderer, ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }
}

} // namespace Rcam4

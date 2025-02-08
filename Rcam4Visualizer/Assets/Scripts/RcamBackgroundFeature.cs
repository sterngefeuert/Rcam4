using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Rcam4 {

sealed class RcamBackgroundPass : ScriptableRenderPass
{
    class PassData { public RcamBackground Driver { get; set; } }

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        if (!Application.isPlaying) return;

        // Controller component reference retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<RcamBackground>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;

        // Render pass building
        using var builder =
          graph.AddRasterRenderPass<PassData>("Rcam BG", out var data);

        data.Driver = driver;

        var resource = context.Get<UniversalResourceData>();
        builder.SetRenderAttachment(resource.activeColorTexture, 0);
        builder.SetRenderAttachmentDepth
          (resource.activeDepthTexture, AccessFlags.Write);

        // Render function registration
        builder.SetRenderFunc
          ((PassData data, RasterGraphContext context)
             => data.Driver.PushDrawCommand(context));
    }
}

public sealed class RcamBackgroundFeature : ScriptableRendererFeature
{
    RcamBackgroundPass _pass;

    public override void Create()
      => _pass = new RcamBackgroundPass
           { renderPassEvent = RenderPassEvent.AfterRenderingOpaques };

    public override void AddRenderPasses
      (ScriptableRenderer renderer, ref RenderingData data)
      => renderer.EnqueuePass(_pass);
}

} // namespace Rcam4

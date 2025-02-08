using UnityEngine;
using UnityEngine.Rendering;

namespace Rcam4 {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Rcam/Rcam Background")]
public sealed class RcamBackground : MonoBehaviour
{
    #region Scene object references

    [SerializeField] FrameDecoder _decoder = null;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Private members

    Material _material;

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => _material = new Material(_shader);

    void OnDestroy()
      => CoreUtils.Destroy(_material);

    #endregion

    #region Public methods

    public bool IsReady
      => _material != null &&
         _decoder.ColorTexture != null &&
         _decoder.DepthTexture != null;

    public void PushDrawCommand
      (UnityEngine.Rendering.RenderGraphModule.RasterGraphContext context)
    {
        var inv_proj = CameraUtil.GetInverseProjection(_decoder.Metadata);
        var inv_view = CameraUtil.GetInverseView(_decoder.Metadata);
        _material.SetVector(ShaderID.InverseProjection, inv_proj);
        _material.SetMatrix(ShaderID.InverseView, inv_view);
        _material.SetTexture(ShaderID.ColorMap, _decoder.ColorTexture);
        _material.SetTexture(ShaderID.DepthMap, _decoder.DepthTexture);
        CoreUtils.DrawFullScreen(context.cmd, _material);
    }

    #endregion
}

} // namespace Rcam4

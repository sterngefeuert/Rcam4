using UnityEngine;

namespace Rcam4 {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Rcam/Rcam Background")]
public sealed class RcamBackground : MonoBehaviour
{
    #region Scene object references

    [SerializeField] FrameDecoder _decoder = null;

    #endregion

    #region Public properties

    public bool IsReady => Properties != null;
    public MaterialPropertyBlock Properties { get; private set; }

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        if (_decoder.ColorTexture == null) return;
        if (_decoder.DepthTexture == null) return;

        if (Properties == null) Properties = new MaterialPropertyBlock();

        var inv_proj = CameraUtil.GetInverseProjection(_decoder.Metadata);
        var inv_view = CameraUtil.GetInverseView(_decoder.Metadata);

        Properties.SetVector(ShaderID.InverseProjection, inv_proj);
        Properties.SetMatrix(ShaderID.InverseView, inv_view);

        Properties.SetTexture(ShaderID.ColorMap, _decoder.ColorTexture);
        Properties.SetTexture(ShaderID.DepthMap, _decoder.DepthTexture);
    }

    #endregion
}

} // namespace Rcam4

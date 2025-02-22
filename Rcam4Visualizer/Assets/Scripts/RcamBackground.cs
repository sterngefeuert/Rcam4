using UnityEngine;

namespace Rcam4 {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Rcam/Rcam Background")]
public sealed class RcamBackground : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField, Range(0, 1)]
    public float BackOpacity { get; set; } = 1;

    [field:SerializeField, Range(0, 1)]
    public float FrontOpacity { get; set; } = 1;

    [field:SerializeField, Range(0, 1)]
    public float TotalOpacity { get; set; } = 1;

    #endregion

    #region Scene object references

    [SerializeField] FrameDecoder _decoder = null;

    #endregion

    #region Public properties

    public bool IsActive => BackOpacity > 0 || FrontOpacity > 0;

    public MaterialPropertyBlock Properties => UpdateMaterialProperties();

    #endregion

    #region Private members

    MaterialPropertyBlock _props;

    MaterialPropertyBlock UpdateMaterialProperties()
    {
        if (_props == null) _props = new MaterialPropertyBlock();
        if (_decoder == null || _decoder.ColorTexture == null) return _props;

        var inv_proj = CameraUtil.GetInverseProjection(_decoder.Metadata);
        var inv_view = CameraUtil.GetInverseView(_decoder.Metadata);

        _props.SetVector(ShaderID.InverseProjection, inv_proj);
        _props.SetMatrix(ShaderID.InverseView, inv_view);

        _props.SetFloat(PropIDs._BackFill, BackOpacity * TotalOpacity);
        _props.SetFloat(PropIDs._FrontFill, FrontOpacity * TotalOpacity);

        _props.SetTexture(ShaderID.ColorMap, _decoder.ColorTexture);
        _props.SetTexture(ShaderID.DepthMap, _decoder.DepthTexture);

        return _props;
    }

    #endregion
}

} // namespace Rcam4

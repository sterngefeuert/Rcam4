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

    [field:SerializeField, ColorUsage(false, true)]
    public Color EffectColor { get; set; } = new Color(1, 1, 1, 1);

    [field:SerializeField, Range(0, 1)]
    public float EffectOpacity { get; set; } = 0;

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

        // Rcam properties
        var inv_proj = CameraUtil.GetInverseProjection(_decoder.Metadata);
        var inv_view = CameraUtil.GetInverseView(_decoder.Metadata);
        _props.SetVector(ShaderID.InverseProjection, inv_proj);
        _props.SetMatrix(ShaderID.InverseView, inv_view);

        _props.SetTexture(ShaderID.ColorMap, _decoder.ColorTexture);
        _props.SetTexture(ShaderID.DepthMap, _decoder.DepthTexture);

        // Other properties
        _props.SetFloat(PropIDs._BackFill, BackOpacity * TotalOpacity);
        _props.SetFloat(PropIDs._FrontFill, FrontOpacity * TotalOpacity);

        var eff_color = EffectColor;
        eff_color.a = EffectOpacity;
        _props.SetColor(PropIDs._EffectColor, eff_color);

        return _props;
    }

    #endregion
}

} // namespace Rcam4

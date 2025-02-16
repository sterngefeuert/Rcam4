using UnityEngine;

namespace Rcam4 {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Rcam/Recolor Effect")]
public sealed class RecolorEffect : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField]
    public bool BackFill { get; set; }

    [field:SerializeField]
    public bool FrontFill { get; set; }

    [field:SerializeField, Range(0, 1)]
    public float BaseHue { get; set; } = 0.5f;

    [field:SerializeField, Range(0, 1)]
    public float Dithering { get; set; } = 0.3f;

    #endregion

    #region Public properties

    public bool IsActive => BackFill || FrontFill;

    public MaterialPropertyBlock Properties => UpdateMaterialProperties();

    #endregion

    #region Coloring algorithm

    Vector4 GetColor(float hoffs, float s, float v)
    {
        var h = (BaseHue + hoffs + 1) % 1;
        return Color.HSVToRGB(h, s, v).linear;
    }

    Matrix4x4 GetColorMatrix
      (Color c1, Color c2, Color c3, Color c4,
       float th1, float th2, float th3)
    {
        c2.a = th1; c3.a = th2; c4.a = th3;
        return new Matrix4x4(c1, c2, c3, c4).transpose;
    }

    Matrix4x4 Palette1
      => GetColorMatrix(GetColor(0.2f, 0.7f, 0.0f),
                        GetColor(0.2f, 0.7f, 0.7f),
                        GetColor(0.3f, 0.3f, 0.9f),
                        GetColor(0.3f, 0.1f, 1.0f),
                        0.1f, 0.5f, 0.95f);

    Matrix4x4 Palette2
      => GetColorMatrix(GetColor( 0.0f, 1.0f, 0.1f),
                        GetColor( 0.4f, 1.0f, 0.7f),
                        GetColor(-0.1f, 0.5f, 0.9f),
                        GetColor(-0.2f, 0.1f, 1.0f),
                        0.1f, 0.5f, 0.95f);

    #endregion

    #region Private members

    MaterialPropertyBlock _props;

    MaterialPropertyBlock UpdateMaterialProperties()
    {
        if (_props == null) _props = new MaterialPropertyBlock();
        _props.SetMatrix("_BgColors", Palette1);
        _props.SetMatrix("_FgColors", Palette2);
        _props.SetFloat("_BackFill", BackFill ? 1 : 0);
        _props.SetFloat("_FrontFill", FrontFill ? 1 : 0);
        _props.SetFloat("_Dithering", Dithering);
        return _props;
    }

    #endregion
}

} // namespace Rcam4

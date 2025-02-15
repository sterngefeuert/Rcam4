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

    [field:SerializeField]
    public Color ContourColor { get; set; } = Color.white;

    [field:SerializeField, Range(0, 1)]
    public float ContourSense { get; set; } = 0.5f;

    [field:SerializeField, Range(0, 1)]
    public float Dithering { get; set; } = 0.3f;

    #endregion

    #region Public properties and methods

    public bool IsActive => BackFill || FrontFill;

    public MaterialPropertyBlock Properties => UpdateMaterialProperties();

    #endregion

    #region Private members

    MaterialPropertyBlock _props;

    Vector4 GetColor(float hoffs, float y, float a)
    {
        var h = (BaseHue + hoffs + 1) % 1;
        var c = Color.HSVToRGB(h, Mathf.Min(1, 2 - y), Mathf.Min(1, y));
        c.a = a;
        return (Vector4)(c.linear);
    }

    Matrix4x4 GetColorMatrix
      (Color c1, Color c2, Color c3, Color c4, float th1, float th2, float th3)
    {
        c2.a = th1; c3.a = th2; c4.a = th3;
        return new Matrix4x4(c1, c2, c3, c4).transpose;
    }

    MaterialPropertyBlock UpdateMaterialProperties()
    {
        if (_props == null) _props = new MaterialPropertyBlock();

        var thresh = (1 - ContourSense) * 0.05f;

        var palette1 = GetColorMatrix
          (Color.black,
           GetColor(0.25f - 0.1f, 0.4f, 0.1f),
           GetColor(0.25f + 0.0f, 1.0f, 0.5f),
           GetColor(0.25f + 0.1f, 1.4f, 0.95f),
           0.1f, 0.5f, 0.95f);

        var palette2 = GetColorMatrix
          (GetColor(-0.1f, 0.1f, 0.00f),
           GetColor( 0.0f, 0.4f, 0.10f),
           GetColor( 0.1f, 1.4f, 0.50f),
           Color.white,
           0.1f, 0.5f, 0.95f);

        _props.SetVector("_ContourThresh", new Vector2(thresh / 2, thresh));
        _props.SetFloat("_Dithering", Dithering);
        _props.SetMatrix("_BgColors", palette1);
        _props.SetMatrix("_FgColors", palette2);
        _props.SetColor("_ContourColor", ContourColor);

        return _props;
    }

    #endregion
}

} // namespace Rcam4

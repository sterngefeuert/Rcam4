using UnityEngine;
using UnityEngine.UIElements;

namespace Rcam4 {

public sealed class Monitor : MonoBehaviour
{
    #region Scene object references

    [SerializeField] FrameEncoder _encoder = null;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Private members

    Blitter _blitter;
    RenderTexture _decoded;
    VisualElement _uiElement;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Blitter object
        _blitter = new Blitter(_shader);

        // Buffer allocation
        _decoded = new RenderTexture(_encoder.Output.descriptor);
        _decoded.wrapMode = TextureWrapMode.Clamp;

        // UI update
        _uiElement = GetComponent<UIDocument>().rootVisualElement.Q("monitor");
        _uiElement.style.backgroundImage = Background.FromRenderTexture(_decoded);
    }

    void OnDestroy()
    {
        _blitter.Dispose();
        Destroy(_decoded);
    }

    void LateUpdate()
    {
        var meta = _encoder.Metadata;

        // Monitor texture update
        var inv_proj = CameraUtil.GetInverseProjection(meta);
        var inv_view = CameraUtil.GetInverseView(meta);
        _blitter.Material.SetVector(ShaderID.DepthRange, meta.DepthRange);
        _blitter.Material.SetVector(ShaderID.InverseProjection, inv_proj);
        _blitter.Material.SetMatrix(ShaderID.InverseView, inv_view);
        _blitter.Run(_encoder.Output, _decoded, 0);

        // Aspect ratio fix
        var aspect = meta.ProjectionMatrix[0, 0] / meta.ProjectionMatrix[1, 1];
        var rect = _uiElement.parent.contentRect;
        var margin = (rect.width - rect.height / aspect) * 0.5f;
        _uiElement.style.right = margin;
        _uiElement.style.left = margin;
    }

    #endregion
}

} // namespace Rcam4

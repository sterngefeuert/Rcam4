using UnityEngine;
using UnityEngine.UIElements;

namespace Rcam4 {

public sealed class DebugMonitor : MonoBehaviour
{
    #region Scene object references

    [SerializeField] FrameDecoder _decoder = null;

    #endregion

    #region Private members

    Label _uiInfoLabel;
    (VisualElement color, VisualElement depth) _uiImages;
    RenderTexture _colorRT;

    string GenerateInfoText(in Metadata data)
    {
        var p = data.CameraPosition;
        var r = data.CameraRotation.eulerAngles;
        var fov = CameraUtil.GetFieldOfView(data);
        return $"Pos: {p.x:F2}, {p.y:F2}, {p.z:F2}\n" +
               $"Rot: {r.x:F0}, {r.y:F0}, {r.z:F0}\n" +
               $"FoV: {fov:F0} deg.";   
    }

    void AssignRenderTextures()
    {
        var color = Background.FromRenderTexture(_decoder.ColorTexture);
        var depth = Background.FromRenderTexture(_decoder.DepthTexture);
        _uiImages.color.style.backgroundImage = color;
        _uiImages.depth.style.backgroundImage = depth;
        _colorRT = _decoder.ColorTexture;
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _uiInfoLabel = root.Q<Label>("info-label");
        _uiImages.color = root.Q("image-color");
        _uiImages.depth = root.Q("image-depth");
    }

    void Update()
    {
        _uiInfoLabel.text = GenerateInfoText(_decoder.Metadata);
        if (_colorRT != _decoder.ColorTexture) AssignRenderTextures();
    }

    #endregion
}

} // namespace Rcam4

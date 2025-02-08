using UnityEngine;
using Klak.Ndi;

namespace Rcam4 {

public sealed class FrameDecoder : MonoBehaviour
{
    #region Scene object references

    [SerializeField] NdiReceiver _ndiReceiver = null;
    [SerializeField] Texture3D _lut = null;

    #endregion

    #region Project asset refnerence

    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Public accessor properties

    public RenderTexture ColorTexture => _decoded.color;
    public RenderTexture DepthTexture => _decoded.depth;
    public ref readonly Metadata Metadata => ref _metadata;

    #endregion

    #region Private members

    Blitter _blitter;
    (RenderTexture color, RenderTexture depth) _decoded;
    Metadata _metadata = Metadata.InitialData;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _blitter = new Blitter(_shader);
        _blitter.Material.SetTexture(ShaderID.LutTexture, _lut);
    }

    void OnDestroy()
    {
        _blitter.Dispose();
        Destroy(_decoded.color);
        Destroy(_decoded.depth);
    }

    void Update()
    {
        DecodeMetadata();
        DecodeImage();
    }

    #endregion

    #region Metadata decoding

    void DecodeMetadata()
    {
        var xml = _ndiReceiver.metadata;
        if (xml == null || xml.Length == 0) return;
        _metadata = Metadata.Deserialize(xml);
    }

    #endregion

    #region Image plane decoding

    void DecodeImage()
    {
        var source = _ndiReceiver.texture;
        if (source == null) return;

        // Lazy initialization
        if (_decoded.color == null) AllocatePlanes(source);

        // Parameters from metadata
        _blitter.Material.SetVector(ShaderID.DepthRange, _metadata.DepthRange);

        // Decoder invocation blit
        _blitter.Run(source, _decoded.color, 0);
        _blitter.Run(source, _decoded.depth, 1);
    }

    void AllocatePlanes(RenderTexture source)
    {
        _decoded.color = RTUtil.AllocColor(source.width / 2, source.height);
        _decoded.depth = RTUtil.AllocHalf(source.width / 2, source.height / 2);
    }

    #endregion
}

} // namespace Rcam4

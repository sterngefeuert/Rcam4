using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Klak.Ndi;

namespace Rcam4 {

public sealed class FrameEncoder : MonoBehaviour
{
    #region Editable attributes

    [Space]
    [SerializeField] ARCameraManager _cameraManager = null;
    [SerializeField] AROcclusionManager _occlusionManager = null;
    [SerializeField] NdiSender _ndiSender = null;
    [Space]
    [SerializeField] float _minDepth = 0.2f;
    [SerializeField] float _maxDepth = 3.2f;
    [Space]
    [SerializeField] RenderTexture _output = null;
    [SerializeField] InputHandle _input = null;

    #endregion

    #region Project asset reference

    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Public accessors

    public RenderTexture Output => _output;

    public Metadata Metadata
      => new Metadata(cameraPosition: _camera.transform.position,
                      cameraRotation: _camera.transform.rotation,
                      projectionMatrix: _projMatrix,
                      depthRange: new Vector2(_minDepth, _maxDepth),
                      inputState: _input.InputState);

    #endregion

    #region Private members

    Camera _camera;
    Blitter _blitter;
    Matrix4x4 _projMatrix = Matrix4x4.identity;
    RenderTexture _encoded;

    #endregion

    #region Camera callbacks

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.textures.Count == 0) return;

        // Y/CbCr textures
        for (var i = 0; i < args.textures.Count; i++)
        {
            var id = args.propertyNameIds[i];
            if (id == ShaderID.TextureY || id == ShaderID.TextureCbCr)
                _blitter.Material.SetTexture(id, args.textures[i]);
        }

        // Projection matrix
        if (args.projectionMatrix.HasValue)
        {
            _projMatrix = args.projectionMatrix.Value;

            // Texture aspect ratio
            var tex1 = args.textures[0];
            var texAspect = (float)tex1.width / tex1.height;

            // Aspect ratio compensation (camera vs. texture)
            var s = texAspect / _camera.aspect;
            _projMatrix[1, 1] *= s;
            _projMatrix[1, 2] *= s;
        }
    }

    void OnOcclusionFrameReceived(AROcclusionFrameEventArgs args)
    {
        // Stencil/depth textures.
        for (var i = 0; i < args.textures.Count; i++)
        {
            var id = args.propertyNameIds[i];
            if (id == ShaderID.HumanStencil ||
                id == ShaderID.EnvironmentDepth ||
                id == ShaderID.EnvironmentDepthConfidence)
                _blitter.Material.SetTexture(id, args.textures[i]);
        }
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Component reference
        _camera = _cameraManager.GetComponent<Camera>();

        // Blitter object
        _blitter = new Blitter(_shader);

        // Muxer buffer allocation
        _encoded = new RenderTexture(_output.descriptor);
        _encoded.wrapMode = TextureWrapMode.Clamp;
        _encoded.Create();
    }

    void OnDestroy()
    {
        _blitter.Dispose();
        Destroy(_encoded);
    }

    void OnEnable()
    {
        // Camera callback setup
        _cameraManager.frameReceived += OnCameraFrameReceived;
        _occlusionManager.frameReceived += OnOcclusionFrameReceived;
    }

    void OnDisable()
    {
        // Camera callback termination
        _cameraManager.frameReceived -= OnCameraFrameReceived;
        _occlusionManager.frameReceived -= OnOcclusionFrameReceived;
    }

    void Update()
    {
        // Metadata update
        _ndiSender.metadata = Metadata.Serialize();

        // Parameter update
        var range = new Vector2(_minDepth, _maxDepth);
        _blitter.Material.SetVector(ShaderID.DepthRange, range);

        // Delayed output
        Graphics.CopyTexture(_encoded, _output);

        // Multiplexer invocation
        _blitter.Run(null, _encoded, 0);
    }

    #endregion
}

} // namespace Rcam4

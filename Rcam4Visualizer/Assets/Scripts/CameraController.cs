using UnityEngine;
using UnityEngine.VFX;
using Klak.Math;
using Klak.Motion;

namespace Rcam4 {

public sealed class CameraController : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField] public float LongFov { get; set; } = 40;
    [field:SerializeField] public float PushSpeed { get; set; } = 4;
    [field:SerializeField] public float PullSpeed { get; set; } = 18;
    [field:SerializeField] public float PlaneScale1st { get; set; } = 0.59f;
    [field:SerializeField] public float PlaneScale3rd { get; set; } = 0.95f;

    #endregion

    #region Scene object references

    [SerializeField] Camera _camera = null;
    [SerializeField] FrameDecoder _decoder = null;
    [SerializeField] SmoothFollow _follower = null;
    [SerializeField] Transform _target1st = null;
    [SerializeField] Transform _target3rd = null;
    [SerializeField] GameObject _vfx = null;
    [SerializeField] Transform _mainPlane = null;

    #endregion

    #region Public methods

    public async void ToggleMode()
    {
        _is1st = !_is1st;

        if (_is1st)
        {
            _follower.enabled = true;
            _follower.positionSpeed = PullSpeed;
            _follower.rotationSpeed = PullSpeed;
            _follower.target = _target1st;

            await Awaitable.WaitForSecondsAsync(0.5f);

            _follower.enabled = false;
            _cameraLinker.enabled = true;
            _background.enabled = true;
            _vfx.SetActive(false);
        }
        else
        {
            _follower.enabled = true;
            _follower.positionSpeed = PushSpeed;
            _follower.rotationSpeed = PushSpeed;
            _follower.target = _target3rd;
            _cameraLinker.enabled = false;
            _background.enabled = false;
            _vfx.SetActive(true);
        }
    }

    #endregion

    #region Private members

    CameraLinker _cameraLinker;
    RcamBackground _background;

    bool _is1st = true;
    (float x, float v) _cameraFov;
    (float x, float v) _planeScale;
    (float x, float v) _vfxScale;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _cameraLinker = _camera.GetComponent<CameraLinker>();
        _background = _camera.GetComponent<RcamBackground>();
    }

    void LateUpdate()
    {
        var cameraFovTarget = _is1st ?
          CameraUtil.GetFieldOfView(_decoder.Metadata) : LongFov;

        var planeScaleTarget = _is1st ? PlaneScale1st : PlaneScale3rd;

        var vfxScaleTarget = _is1st ? 0 : 1;

        var speed = _is1st ? PullSpeed : PushSpeed;

        _cameraFov = CdsTween.Step(_cameraFov, cameraFovTarget, speed);
        _planeScale = CdsTween.Step(_planeScale, planeScaleTarget, speed);
        _vfxScale = CdsTween.Step(_vfxScale, vfxScaleTarget, speed);

        _camera.fieldOfView = _cameraFov.x;
        _mainPlane.localScale = Vector3.one * _planeScale.x;

        foreach (var vfx in _vfx.GetComponentsInChildren<VisualEffect>())
        {
            if (vfx.gameObject.name == "Points") continue;
            vfx.SetFloat("Throttle", _vfxScale.x);
        }
    }

    #endregion
}

} // namespace Rcam4

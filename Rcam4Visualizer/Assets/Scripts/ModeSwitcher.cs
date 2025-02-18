using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Klak.Motion;

namespace Rcam4 {

// Mode Switcher: 1st person view mode <-> 3rd person view mode
public sealed class ModeSwitcher : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField] public float Fov3rd { get; set; } = 40;
    [field:SerializeField] public float TimeTo1st { get; set; } = 0.5f;
    [field:SerializeField] public float TimeTo3rd { get; set; } = 2;
    [field:SerializeField] public float PlaneScale1st { get; set; } = 0.59f;
    [field:SerializeField] public float PlaneScale3rd { get; set; } = 0.95f;

    #endregion

    #region Scene object references

    [SerializeField] Camera _camera = null;
    [SerializeField] FrameDecoder _decoder = null;
    [SerializeField] SmoothFollow _follower = null;
    [SerializeField] Transform _target1st = null;
    [SerializeField] VisualEffect[] _pointClouds = null;
    [SerializeField] Transform _mainPlane = null;

    #endregion

    #region Public methods

    public async void ToggleMode()
    {
        // Reentrance guard
        if (_inTransition) return;
        _inTransition = true;

        // Mode flip
        _is1st = !_is1st;

        if (_is1st)
        {
            // 3rd person view mode -> 1st person view mode
            _follower.enabled = false;

            var tweenFrom = _camera.transform.position;
            var tweenFromR = _camera.transform.rotation;

            for (var t = 0.0f; t < 1;)
            {
                t = math.saturate(t + Time.deltaTime / TimeTo1st);
                var s = math.smoothstep(0, 1, 1 - t);
                _camera.transform.position = math.lerp(_target1st.position, tweenFrom, s);
                _camera.transform.rotation = math.slerp(_target1st.rotation, tweenFromR, s);
                UpdateTweenParams(s);
                await Awaitable.NextFrameAsync();
            }

            UpdateTweenParams(0);

            _cameraLinker.enabled = true;
            _background.enabled = true;
        }
        else
        {
            // 1st person view mode -> 3rd person view mode
            _follower.enabled = true;
            _cameraLinker.enabled = false;
            _background.enabled = false;

            for (var t = 0.0f; t < 1;)
            {
                t = math.saturate(t + Time.deltaTime / TimeTo3rd);
                UpdateTweenParams(math.smoothstep(0, 1, t));
                await Awaitable.NextFrameAsync();
            }

            UpdateTweenParams(1);
        }

        _inTransition = false;
    }

    #endregion

    #region Private members

    CameraLinker _cameraLinker;
    RcamBackground _background;
    bool _is1st, _inTransition;

    void UpdateTweenParams(float s)
    {
        // Camera FOV
        var remote = CameraUtil.GetFieldOfView(_decoder.Metadata);
        _camera.fieldOfView = math.lerp(remote, Fov3rd, s);

        // Main projector plane scale
        _mainPlane.localScale =
          Vector3.one * math.lerp(PlaneScale1st, PlaneScale3rd, s);

        // Point cloud VFX
        if (s == 0)
        {
            for (var i = 0; i < _pointClouds.Length; i++)
                _pointClouds[i].enabled = false;
        }
        else
        {
            for (var i = 0; i < _pointClouds.Length; i++)
            {
                _pointClouds[i].enabled = true;
                _pointClouds[i].SetFloat("Throttle", i == 0 ? 1 : s);
            }
        }
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _cameraLinker = _camera.GetComponent<CameraLinker>();
        _background = _camera.GetComponent<RcamBackground>();
        ToggleMode(); // Starting from the 1st person view mode
    }

    #endregion
}

} // namespace Rcam4

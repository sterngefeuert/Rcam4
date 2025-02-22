using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Klak.Motion;

namespace Rcam4 {

// Scene Mode Switcher: 1st person view mode <-> 3rd person view mode
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

    #region Public runtime properties

    public bool In3rdPersonMode { get => _in3rd; set => TryChangeMode(value); }

    #endregion

    #region Private members

    CameraLinker _cameraLinker;
    RcamBackground _background;
    bool _in3rd, _inTransition;

    void TryChangeMode(bool requested)
    {
        if (requested == _in3rd || _inTransition) return;
        _in3rd = requested;
        AsyncUtil.Forget(_in3rd ? TransitionTo3rd() : TransitionTo1st());
    }

    // 1st person view mode -> 3rd person view mode
    public async Awaitable TransitionTo3rd()
    {
        _inTransition = true;
        _follower.enabled = true;
        _cameraLinker.enabled = false;

        for (var t = 0.0f; t < 1;)
        {
            t = math.saturate(t + Time.deltaTime / TimeTo3rd);
            UpdateTweenParams(t);
            await Awaitable.NextFrameAsync();
        }

        _background.enabled = false;
        _inTransition = false;
    }

    // 3rd person view mode -> 1st person view mode
    public async Awaitable TransitionTo1st()
    {
        _inTransition = true;
        _follower.enabled = false;
        _background.enabled = true;

        var p0 = _camera.transform.position;
        var r0 = _camera.transform.rotation;

        for (var t = 0.0f; t < 1;)
        {
            t = math.saturate(t + Time.deltaTime / TimeTo1st);
            var s = math.smoothstep(0, 1, 1 - t);
            _camera.transform.position = math.lerp(_target1st.position, p0, s);
            _camera.transform.rotation = math.slerp(_target1st.rotation, r0, s);
            UpdateTweenParams(1 - t);
            await Awaitable.NextFrameAsync();
        }

        _cameraLinker.enabled = true;
        _inTransition = false;
    }

    void UpdateTweenParams(float t)
    {
        var fastT = math.smoothstep(0, 0.2f, t);
        var slowT = math.smoothstep(0, 1, t);

        // Camera FOV
        var remote = CameraUtil.GetFieldOfView(_decoder.Metadata);
        _camera.fieldOfView = math.lerp(remote, Fov3rd, slowT);

        // Main projector plane scale
        _mainPlane.localScale =
          Vector3.one * math.lerp(PlaneScale1st, PlaneScale3rd, slowT);

        // Background opacity
        _background.Opacity = 1 - fastT;

        // Point cloud VFX
        if (t == 0)
        {
            for (var i = 0; i < _pointClouds.Length; i++)
                _pointClouds[i].enabled = false;
        }
        else
        {
            for (var i = 0; i < _pointClouds.Length; i++)
            {
                _pointClouds[i].enabled = true;
                _pointClouds[i].SetFloat("Throttle", i == 0 ? 1 : slowT);
            }
        }
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _cameraLinker = _camera.GetComponent<CameraLinker>();
        _background = _camera.GetComponent<RcamBackground>();
    }

    #endregion
}

} // namespace Rcam4

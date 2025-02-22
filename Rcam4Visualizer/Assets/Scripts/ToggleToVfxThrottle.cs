using UnityEngine;
using UnityEngine.VFX;

namespace Rcam4 {

public sealed class ToggleToVfxThrottle : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _toggleIndex = 0;
    [SerializeField] VisualEffect _target = null;
    [SerializeField] float _transitionTime = 3;
    [SerializeField] float _delayToSleep = 1;

    #endregion

    #region Private members

    float _throttle, _zeroTimer;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        var flag = _handle.GetToggle(_toggleIndex);

        var dt = Time.deltaTime;
        var delta = (flag ? 1 : -1) * dt / _transitionTime;
        _throttle = Mathf.Clamp01(_throttle + delta);
        _zeroTimer = _throttle > 0 ? 0 : _zeroTimer + dt;

        _target.enabled = _zeroTimer <= _delayToSleep;
        _target.SetFloat(PropIDs.Throttle, _throttle);
    }

    #endregion
}

} // namespace Rcam4

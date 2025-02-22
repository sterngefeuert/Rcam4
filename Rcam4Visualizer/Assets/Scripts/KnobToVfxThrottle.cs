using UnityEngine;
using UnityEngine.VFX;

namespace Rcam4 {

public sealed class KnobToVfxThrottle : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _knobIndex = 0;
    [SerializeField] VisualEffect _target = null;
    [SerializeField] float _delayToSleep = 1;

    #endregion

    #region Private members

    float _zeroTimer;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        var input = _handle.GetKnob(_knobIndex);
        _zeroTimer = input > 0.01f ? 0 : _zeroTimer + Time.deltaTime;
        _target.enabled = _zeroTimer <= _delayToSleep;
        _target.SetFloat(PropIDs.Throttle, input);
    }

    #endregion
}

} // namespace Rcam4
